#!/usr/bin/env bash
#
# uninstall.sh — Remove a user-scoped grok-search-cli installation.
#
# Usage:
#   ./uninstall.sh               # default user-scoped install directory
#   ./uninstall.sh --dir ~/bin   # custom install directory
#
# The script removes only installer-managed CLI files. It does not edit shell
# profiles, PATH entries, or credential configuration outside the install
# directory.
#

set -euo pipefail

# ---------------------------------------------------------------------------
# Shell detection and PATH management
# ---------------------------------------------------------------------------

detect_shell() {
    local shell_name=""

    if command -v ps >/dev/null 2>&1; then
        shell_name="$(ps -p "$PPID" -o comm= 2>/dev/null || true)"
        shell_name="${shell_name##*/}"
    fi

    if [[ -z "$shell_name" && -n "${SHELL:-}" ]]; then
        shell_name="${SHELL##*/}"
    fi

    case "${shell_name}" in
        bash|zsh|fish) echo "$shell_name"; return 0 ;;
        sh|dash|ksh)   echo "bash";      return 0 ;;
        *)             echo "bash";      return 0 ;;
    esac
}

get_profile_path() {
    local shell_name="$1"
    case "$shell_name" in
        bash) echo "${HOME}/.bashrc" ;;
        zsh)  echo "${HOME}/.zshrc" ;;
        fish)
            mkdir -p "${HOME}/.config/fish"
            echo "${HOME}/.config/fish/config.fish"
            ;;
    esac
}

backup_profile() {
    local profile_path="$1"
    local backup_path="${profile_path}.grok-search-cli-backup-$(date +%Y%m%d%H%M%S)"
    if [[ -f "$profile_path" ]]; then
        cp "$profile_path" "$backup_path"
        echo "Backup created: ${backup_path}"
    fi
}

remove_from_path() {
    local install_dir="$1"
    local shell_name
    local profile_path
    local removed=0

    shell_name="$(detect_shell)"
    profile_path="$(get_profile_path "$shell_name")"

    # Remove sentinel-wrapped block from profile if present
    if [[ -f "$profile_path" ]] && grep -q "^# grok-search-cli PATH begin" "$profile_path" 2>/dev/null; then
        backup_profile "$profile_path"

        # Delete from '# grok-search-cli PATH begin' through '# grok-search-cli PATH end' inclusive
        # macOS BSD sed requires a backup extension arg; Linux GNU sed does not
        if [[ "$(uname -s)" == "Darwin" ]]; then
            sed -i '' '/^# grok-search-cli PATH begin/,/^# grok-search-cli PATH end/d' "$profile_path"
        else
            sed -i '/^# grok-search-cli PATH begin/,/^# grok-search-cli PATH end/d' "$profile_path"
        fi

        # Clean up blank lines left by removal
        if command -v perl >/dev/null 2>&1; then
            perl -i -0 -pe 's/\n{3,}/\n\n/g' "$profile_path" 2>/dev/null || true
        fi

        echo "Removed grok-search-cli PATH entry from ${profile_path}."
        removed=1
    fi

    # Check for residual manual entries in runtime PATH
    case ":${PATH}:" in
        *":${install_dir}:"*)
            echo "NOTE: The install directory '${install_dir}' is still in your current PATH." >&2
            echo "      It may be set in a shell profile that was not managed by the installer." >&2
            echo "      Remove it manually if no longer needed." >&2
            ;;
    esac

    if [[ $removed -eq 0 ]]; then
        echo "No managed grok-search-cli PATH entry was present to remove."
    fi
}

# ---------------------------------------------------------------------------
# Interactive credential cleanup
# ---------------------------------------------------------------------------

get_credential_store_path() {
    if [[ -n "${XDG_CONFIG_HOME:-}" ]]; then
        echo "${XDG_CONFIG_HOME}/grok-search-cli/credentials.env"
    else
        echo "${HOME}/.config/grok-search-cli/credentials.env"
    fi
}

interactive_credential_cleanup() {
    local cred_files=()
    local cred_store
    local response

    cred_store="$(get_credential_store_path)"

    # Check managed credential store
    if [[ -f "$cred_store" ]]; then
        cred_files+=("managed:${cred_store}")
    fi

    # Check $HOME/.env for XAI_API_KEY
    if [[ -f "${HOME}/.env" ]] && grep -q "XAI_API_KEY" "${HOME}/.env" 2>/dev/null; then
        cred_files+=("home:${HOME}/.env")
    fi

    # Check current directory .env for XAI_API_KEY
    if [[ -f ".env" ]] && grep -q "XAI_API_KEY" ".env" 2>/dev/null; then
        cred_files+=("cwd:$(pwd)/.env")
    fi

    if [[ ${#cred_files[@]} -eq 0 ]]; then
        echo "No API key files (.env or credential store) were found."
        return 0
    fi

    echo ""
    echo "=== Credential Cleanup ==="

    for entry in "${cred_files[@]}"; do
        local entry_type="${entry%%:*}"
        local entry_path="${entry#*:}"

        case "$entry_type" in
            managed)
                echo ""
                echo "Managed credential store detected: ${entry_path}"
                printf "Clear managed credentials (equivalent to 'grok-search-cli auth logout')? [y/N] "
                if read -r response; then
                    if [[ "$response" =~ ^[Yy] ]]; then
                        rm -f "$entry_path"
                        echo "Managed credential store cleared."
                    else
                        echo "Skipped. Clear manually with: grok-search-cli auth logout"
                    fi
                fi
                ;;
            home|cwd)
                echo ""
                echo "API key file detected: ${entry_path}"
                printf "Delete this file? [y/N] "
                if read -r response; then
                    if [[ "$response" =~ ^[Yy] ]]; then
                        rm -f "$entry_path"
                        echo "Deleted: ${entry_path}"
                    else
                        echo "Skipped."
                    fi
                fi
                ;;
        esac
    done

    echo ""
    echo "Note: XAI_API_KEY environment variable (if set) is managed outside this script."
    echo "      To unset it, remove it from your shell profile or terminal configuration."
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --dir)
            INSTALL_DIR="$2"
            shift 2
            ;;
        --help|-h)
            echo "Usage: $0 [--dir <path>]"
            echo ""
            echo "  --dir <path>      Installation directory."
            echo "                    Default: \$HOME/.grok-search-cli/bin"
            exit 0
            ;;
        *)
            echo "error: unknown argument '$1'" >&2
            echo "Usage: $0 [--dir <path>]" >&2
            exit 1
            ;;
    esac
done

if [[ -z "$INSTALL_DIR" ]]; then
    INSTALL_DIR="${HOME}/.grok-search-cli/bin"
fi

MANAGED_PATHS=(
    "${INSTALL_DIR}/grok-search-cli"
)

REMOVED_PATHS=()
REMOVED_DIRECTORY=0

echo "=== grok-search-cli Uninstaller ==="
echo ""
echo "Install target: ${INSTALL_DIR}"

if [[ -d "$INSTALL_DIR" ]]; then
    for managed_path in "${MANAGED_PATHS[@]}"; do
        if [[ -f "$managed_path" ]]; then
            rm -f "$managed_path"
            REMOVED_PATHS+=("$managed_path")
        fi
    done

    if [[ -z "$(find "$INSTALL_DIR" -mindepth 1 -maxdepth 1 -print -quit)" ]]; then
        rmdir "$INSTALL_DIR"
        REMOVED_DIRECTORY=1
    fi
fi

echo ""
if [[ ${#REMOVED_PATHS[@]} -gt 0 ]]; then
    echo "Removed managed grok-search-cli files:"
    for removed_path in "${REMOVED_PATHS[@]}"; do
        echo "  ${removed_path}"
    done
else
    echo "No managed grok-search-cli files were present to remove."
fi

if [[ $REMOVED_DIRECTORY -eq 1 ]]; then
    echo "Removed empty install directory: ${INSTALL_DIR}"
elif [[ -d "$INSTALL_DIR" ]]; then
    echo "Install directory left in place because it still contains non-managed files."
fi

echo ""
remove_from_path "$INSTALL_DIR"

interactive_credential_cleanup
echo ""
echo "Uninstall complete!"