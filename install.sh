#!/usr/bin/env bash
#
# install.sh — Install or upgrade grok-search-cli from GitHub Release assets
#               into a user-scoped location.
#
# Usage:
#   ./install.sh                     # latest release
#   ./install.sh --version v1.0.0    # pinned version
#   ./install.sh --dir ~/bin          # custom install directory
#
# No credentials or secrets are collected during installation.
# After installation, set up credentials via:
#   grok-search-cli auth login
#

set -euo pipefail

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------
REPO="marble810/grok-search-cli"
VERSION=""
INSTALL_DIR=""

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
        --help|-h)
            echo "Usage: $0 [--version <tag>] [--dir <path>]"
            echo ""
            echo "  --version <tag>   Install a specific release (e.g., v1.0.0)."
            echo "                    Defaults to the latest stable release."
            echo "  --dir <path>      Installation directory."
            echo "                    Default: \$HOME/.grok-search-cli/bin"
            exit 0
            ;;
        *)
            echo "error: unknown argument '$1'" >&2
            echo "Usage: $0 [--version <tag>] [--dir <path>]" >&2
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
BASE_URL="https://github.com/${REPO}/releases/download/${VERSION}"

ARCHIVE_URL="${BASE_URL}/${ARCHIVE_NAME}"
CHECKSUM_URL="${BASE_URL}/${CHECKSUM_NAME}"

# ---------------------------------------------------------------------------
# Download
# ---------------------------------------------------------------------------
TMP_DIR="$(mktemp -d /tmp/grok-search-cli-install.XXXXXX)"
cleanup() { rm -rf "$TMP_DIR"; }
trap cleanup EXIT

echo ""
echo "Downloading archive..."
curl -sSfL "$ARCHIVE_URL" -o "${TMP_DIR}/${ARCHIVE_NAME}"

echo "Downloading checksum..."
curl -sSfL "$CHECKSUM_URL" -o "${TMP_DIR}/${CHECKSUM_NAME}"

# ---------------------------------------------------------------------------
# Verify checksum
# ---------------------------------------------------------------------------
echo "Verifying checksum..."
# sha256sum produces: "<hash>  <filename>"
(cd "$TMP_DIR" && sha256sum --check --status "$CHECKSUM_NAME")
echo "Checksum verified."

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
# PATH guidance
# ---------------------------------------------------------------------------
case ":${PATH}:" in
    *":${INSTALL_DIR}:"*)
        echo "Install directory is already in your PATH."
        ;;
    *)
        echo ""
        echo "NOTE: Add the install directory to your PATH. Run one of these commands:" >&2
        echo ""
        echo "  # bash/zsh"
        echo "  echo 'export PATH=\"${INSTALL_DIR}:\$PATH\"' >> ~/.bashrc"
        echo "  source ~/.bashrc"
        echo ""
        echo "  # zsh"
        echo "  echo 'export PATH=\"${INSTALL_DIR}:\$PATH\"' >> ~/.zshrc"
        echo "  source ~/.zshrc"
        echo ""
        echo "  # fish"
        echo "  fish_add_path ${INSTALL_DIR}"
        ;;
esac

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
