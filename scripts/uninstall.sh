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

INSTALL_DIR=""

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
case ":${PATH}:" in
    *":${INSTALL_DIR}:"*)
        echo "NOTE: The install directory may still be present in your PATH or shell profile. Remove '${INSTALL_DIR}' manually if you no longer want that entry."
        ;;
    *)
        echo "No current PATH entry was detected for the install directory. If you added it to a shell profile manually, remove that entry if it is no longer needed."
        ;;
esac

echo "Credential configuration via XAI_API_KEY, .env files, or auth-managed storage was not modified."
echo ""
echo "Uninstall complete!"