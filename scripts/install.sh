#!/usr/bin/env bash
#
# install.sh — Install or upgrade grok-search-cli from GitHub Release assets
#               into a user-scoped location.
#
# Usage:
#   ./install.sh                     # latest release
#   ./install.sh --version v1.0.0    # pinned version
#   ./install.sh --dir ~/bin          # custom install directory
#   ./install.sh --version v1.0.0 --asset-dir ./artifacts/local-release
#
# No credentials or secrets are collected during installation.
# After installation, set up credentials via:
#   grok-search-cli auth login
#

set -euo pipefail

download_with_retry() {
    local url="$1"
    local output_path="$2"
    local description="$3"
    local max_attempts="${4:-3}"
    local attempt

    for ((attempt=1; attempt<=max_attempts; attempt++)); do
        rm -f "$output_path"
        if curl -fsSL "$url" -o "$output_path"; then
            return 0
        fi

        if [[ "$attempt" -eq "$max_attempts" ]]; then
            echo "error: failed to download ${description} from ${url} after ${max_attempts} attempts" >&2
            return 1
        fi

        echo "warning: download attempt ${attempt}/${max_attempts} failed for ${description} from ${url}. Retrying..." >&2
    done
}

compute_sha256() {
    local file_path="$1"

    if command -v sha256sum >/dev/null 2>&1; then
        sha256sum "$file_path" | awk '{print tolower($1)}'
        return 0
    fi

    if command -v shasum >/dev/null 2>&1; then
        shasum -a 256 "$file_path" | awk '{print tolower($1)}'
        return 0
    fi

    echo "error: neither sha256sum nor shasum is available for checksum verification" >&2
    return 1
}

resolve_expected_hash() {
    local checksum_path="$1"
    local archive_name="$2"

    awk -v archive_name="$archive_name" '
        {
            hash=$1
            $1=""
            sub(/^[[:space:]]+[*]?/, "", $0)
            sub(/.*\//, "", $0)
            if ($0 == archive_name && hash ~ /^[A-Fa-f0-9]{64}$/) {
                print tolower(hash)
                found=1
                exit
            }
        }
        END {
            if (!found) {
                exit 1
            }
        }
    ' "$checksum_path"
}

# ---------------------------------------------------------------------------
# Shell detection and PATH management
# ---------------------------------------------------------------------------

detect_shell() {
    local shell_name=""

    # Prefer the parent process name to catch cases like 'bash' inside zsh
    if command -v ps >/dev/null 2>&1; then
        shell_name="$(ps -p "$PPID" -o comm= 2>/dev/null || true)"
        shell_name="${shell_name##*/}"
    fi

    # Fall back to $SHELL if parent process detection failed
    if [[ -z "$shell_name" && -n "${SHELL:-}" ]]; then
        shell_name="${SHELL##*/}"
    fi

    # Normalise detected shell
    case "${shell_name}" in
        bash|zsh|fish)
            echo "$shell_name"
            return 0
            ;;
        sh|dash|ksh)
            # Bourne-derived shells default to bash profile
            echo "bash"
            return 0
            ;;
        *)
            echo "bash"
            return 0
            ;;
    esac
}

get_profile_path() {
    local shell_name="$1"
    local profile_path=""

    case "$shell_name" in
        bash)
            profile_path="${HOME}/.bashrc"
            ;;
        zsh)
            profile_path="${HOME}/.zshrc"
            ;;
        fish)
            profile_path="${HOME}/.config/fish/config.fish"
            mkdir -p "$(dirname "$profile_path")"
            ;;
    esac

    echo "$profile_path"
}

backup_profile() {
    local profile_path="$1"
    local backup_path="${profile_path}.grok-search-cli-backup-$(date +%Y%m%d%H%M%S)"

    if [[ -f "$profile_path" ]]; then
        cp "$profile_path" "$backup_path"
        echo "Backup created: ${backup_path}"
    fi
}

add_to_path() {
    local install_dir="$1"
    local shell_name
    local profile_path

    shell_name="$(detect_shell)"
    profile_path="$(get_profile_path "$shell_name")"

    # Check if the sentinel block already exists
    if [[ -f "$profile_path" ]] && grep -q "^# grok-search-cli PATH begin" "$profile_path" 2>/dev/null; then
        echo "PATH entry already configured in ${profile_path}."
        return 0
    fi

    backup_profile "$profile_path"

    cat >> "$profile_path" <<-EOF

# grok-search-cli PATH begin
if [[ ":\$PATH:" != *:"${install_dir}":* ]]; then
    export PATH="${install_dir}:\$PATH"
fi
# grok-search-cli PATH end
EOF

    echo "Added grok-search-cli to PATH in ${profile_path}."
    echo "Restart your terminal or run 'source ${profile_path}' to apply."
}

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------
REPO="marble810/grok-search-cli"
VERSION=""
INSTALL_DIR=""
ASSET_DIR=""

# ---------------------------------------------------------------------------
# Parse arguments
# ---------------------------------------------------------------------------
while [[ $# -gt 0 ]]; do
    case "$1" in
        --version)
            VERSION="$2"
            shift 2
            ;;
        --dir)
            INSTALL_DIR="$2"
            shift 2
            ;;
        --asset-dir)
            ASSET_DIR="$2"
            shift 2
            ;;
        --help|-h)
            echo "Usage: $0 [--version <tag>] [--dir <path>] [--asset-dir <path>]"
            echo ""
            echo "  --version <tag>   Install a specific release (e.g., v1.0.0)."
            echo "                    Defaults to the latest stable release."
            echo "  --dir <path>      Installation directory."
            echo "                    Default: \$HOME/.grok-search-cli/bin"
            echo "  --asset-dir <path>"
            echo "                    Local directory with release-like archive and checksum files."
            echo "                    Requires --version and skips GitHub downloads."
            exit 0
            ;;
        *)
            echo "error: unknown argument '$1'" >&2
            echo "Usage: $0 [--version <tag>] [--dir <path>] [--asset-dir <path>]" >&2
            exit 1
            ;;
    esac
done

# ---------------------------------------------------------------------------
# Platform detection
# ---------------------------------------------------------------------------
detect_rid() {
    local os arch

    case "$(uname -s)" in
        Linux)  os="linux" ;;
        Darwin) os="osx"   ;;
        *)
            echo "error: unsupported OS '$(uname -s)'" >&2
            exit 1
            ;;
    esac

    arch="$(uname -m)"
    case "$arch" in
        x86_64|amd64) arch="x64"  ;;
        aarch64|arm64) arch="arm64" ;;
        *)
            echo "error: unsupported architecture '$arch'" >&2
            exit 1
            ;;
    esac

    # Only publish osx-x64 and osx-arm64; linux-x64; map arm64 Linux to x64 for now
    if [[ "$os" == "linux" && "$arch" == "arm64" ]]; then
        # The project targets linux-x64 in v1; arm64 Linux is not yet packaged.
        # Fall back to x64 and let the user know the binary may not run.
        echo "warning: linux-arm64 is not a supported release target in v1;" >&2
        echo "warning: falling back to linux-x64 — the binary may not run on this host." >&2
        arch="x64"
    fi

    echo "${os}-${arch}"
}

RID="$(detect_rid)"
echo "Platform RID: ${RID}"

# ---------------------------------------------------------------------------
# Resolve installation directory
# ---------------------------------------------------------------------------
if [[ -z "$INSTALL_DIR" ]]; then
    INSTALL_DIR="${HOME}/.grok-search-cli/bin"
fi
mkdir -p "$INSTALL_DIR"

echo "Install target: ${INSTALL_DIR}"

# ---------------------------------------------------------------------------
# Resolve version
# ---------------------------------------------------------------------------
if [[ -n "$ASSET_DIR" && -z "$VERSION" ]]; then
    echo "error: local asset installs require --version so the expected archive name is deterministic" >&2
    exit 1
fi

if [[ -z "$VERSION" ]]; then
    echo "Fetching latest release version from GitHub..."
    VERSION="$(curl -sSfL "https://api.github.com/repos/${REPO}/releases/latest" \
        | grep '"tag_name"' \
        | sed 's/.*"tag_name": "\(.*\)",*/\1/')"
    if [[ -z "$VERSION" ]]; then
        echo "error: failed to resolve latest release version" >&2
        exit 1
    fi
    echo "Latest release: ${VERSION}"
else
    echo "Requested version: ${VERSION}"
fi

# ---------------------------------------------------------------------------
# Build download URLs
# ---------------------------------------------------------------------------
ARCHIVE_NAME="grok-search-cli_${VERSION}_${RID}.tar.gz"
CHECKSUM_NAME="grok-search-cli_${VERSION}_${RID}.sha256"
COMBINED_CHECKSUM_NAME="checksums_${VERSION}.txt"
if [[ -n "$ASSET_DIR" ]]; then
    if [[ ! -d "$ASSET_DIR" ]]; then
        echo "error: local asset directory not found: ${ASSET_DIR}" >&2
        exit 1
    fi

    ASSET_DIR="$(cd "$ASSET_DIR" && pwd)"
    ARCHIVE_SOURCE="${ASSET_DIR}/${ARCHIVE_NAME}"
    CHECKSUM_SOURCE="${ASSET_DIR}/${CHECKSUM_NAME}"
    COMBINED_CHECKSUM_SOURCE="${ASSET_DIR}/${COMBINED_CHECKSUM_NAME}"

    if [[ ! -f "$ARCHIVE_SOURCE" ]]; then
        echo "error: missing local asset '${ARCHIVE_NAME}' in ${ASSET_DIR}" >&2
        exit 1
    fi

    if [[ ! -f "$CHECKSUM_SOURCE" && ! -f "$COMBINED_CHECKSUM_SOURCE" ]]; then
        echo "error: missing local asset '${CHECKSUM_NAME}' or '${COMBINED_CHECKSUM_NAME}' in ${ASSET_DIR}" >&2
        exit 1
    fi

    echo "Asset source: local directory ${ASSET_DIR}"
else
    BASE_URL="https://github.com/${REPO}/releases/download/${VERSION}"
    ARCHIVE_URL="${BASE_URL}/${ARCHIVE_NAME}"
    CHECKSUM_URL="${BASE_URL}/${CHECKSUM_NAME}"
    COMBINED_CHECKSUM_URL="${BASE_URL}/${COMBINED_CHECKSUM_NAME}"
    echo "Asset source: GitHub Releases (${REPO})"
fi

# ---------------------------------------------------------------------------
# Download
# ---------------------------------------------------------------------------
TMP_DIR="$(mktemp -d /tmp/grok-search-cli-install.XXXXXX)"
cleanup() { rm -rf "$TMP_DIR"; }
trap cleanup EXIT
DOWNLOADED_CHECKSUM_PATH="${TMP_DIR}/${CHECKSUM_NAME}"
CHECKSUM_SOURCE_LABEL=""

echo ""
if [[ -n "$ASSET_DIR" ]]; then
    echo "Copying local archive..."
    cp "$ARCHIVE_SOURCE" "${TMP_DIR}/${ARCHIVE_NAME}"

    if [[ -f "$CHECKSUM_SOURCE" ]]; then
        echo "Copying local checksum..."
        cp "$CHECKSUM_SOURCE" "$DOWNLOADED_CHECKSUM_PATH"
        CHECKSUM_SOURCE_LABEL="direct checksum"
    else
        echo "Copying combined checksum manifest..."
        cp "$COMBINED_CHECKSUM_SOURCE" "$DOWNLOADED_CHECKSUM_PATH"
        CHECKSUM_SOURCE_LABEL="combined checksum manifest"
    fi
else
    echo "Downloading archive..."
    download_with_retry "$ARCHIVE_URL" "${TMP_DIR}/${ARCHIVE_NAME}" "archive"

    echo "Downloading checksum..."
    if download_with_retry "$CHECKSUM_URL" "$DOWNLOADED_CHECKSUM_PATH" "checksum"; then
        CHECKSUM_SOURCE_LABEL="direct checksum"
    else
        echo "warning: direct checksum download failed. Falling back to combined checksum manifest..." >&2
        download_with_retry "$COMBINED_CHECKSUM_URL" "$DOWNLOADED_CHECKSUM_PATH" "combined checksum manifest"
        CHECKSUM_SOURCE_LABEL="combined checksum manifest"
    fi
fi

# ---------------------------------------------------------------------------
# Verify checksum
# ---------------------------------------------------------------------------
echo "Verifying checksum..."
EXPECTED_HASH="$(resolve_expected_hash "$DOWNLOADED_CHECKSUM_PATH" "$ARCHIVE_NAME")" || {
    echo "error: checksum entry for '${ARCHIVE_NAME}' not found in ${DOWNLOADED_CHECKSUM_PATH}" >&2
    exit 1
}
ACTUAL_HASH="$(compute_sha256 "${TMP_DIR}/${ARCHIVE_NAME}")"
if [[ "$EXPECTED_HASH" != "$ACTUAL_HASH" ]]; then
    echo "error: checksum mismatch. expected ${EXPECTED_HASH}, got ${ACTUAL_HASH}" >&2
    exit 1
fi
echo "Checksum verified using ${CHECKSUM_SOURCE_LABEL}."

# ---------------------------------------------------------------------------
# Extract binary
# ---------------------------------------------------------------------------
echo "Extracting..."
tar xzf "${TMP_DIR}/${ARCHIVE_NAME}" -C "$TMP_DIR"
# The archive contains a single binary named "grok-search-cli"
BINARY="${TMP_DIR}/grok-search-cli"
if [[ ! -f "$BINARY" ]]; then
    echo "error: binary 'grok-search-cli' not found in archive" >&2
    exit 1
fi
chmod +x "$BINARY"
cp "$BINARY" "${INSTALL_DIR}/grok-search-cli"

echo ""
echo "grok-search-cli ${VERSION} installed to: ${INSTALL_DIR}"

# ---------------------------------------------------------------------------
# PATH configuration
# ---------------------------------------------------------------------------
add_to_path "$INSTALL_DIR"

# ---------------------------------------------------------------------------
# Credential setup handoff (no secrets collected during install)
# ---------------------------------------------------------------------------
echo ""
echo "=== Next Steps: Credential Setup ==="
echo "grok-search-cli does NOT collect API keys during installation."
echo "To set up your xAI API key, run the following command after installation:"
echo ""
echo "  grok-search-cli auth login"
echo ""
echo "You can also configure credentials via the XAI_API_KEY environment variable"
echo "or a .env file in your working directory."
echo ""
echo "Installation complete!"
