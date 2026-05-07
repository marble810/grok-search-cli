#!/usr/bin/env bash

set -euo pipefail

VERSION=""
RUNTIME_ID=""
OUTPUT_DIR=""

usage() {
    echo "Usage: $0 --version <tag> [--rid <runtime-id>] [--output-dir <path>]"
    echo ""
    echo "  --version <tag>      Version tag to encode in archive names (e.g. v1.0.0)."
    echo "  --rid <runtime-id>   Runtime identifier to package. Defaults to the current host RID."
    echo "  --output-dir <path>  Directory where packaged assets are written."
}

detect_rid() {
    local os arch

    case "$(uname -s)" in
        Linux) os="linux" ;;
        Darwin) os="osx" ;;
        *)
            echo "error: unsupported OS '$(uname -s)'" >&2
            exit 1
            ;;
    esac

    case "$(uname -m)" in
        x86_64|amd64) arch="x64" ;;
        aarch64|arm64) arch="arm64" ;;
        *)
            echo "error: unsupported architecture '$(uname -m)'" >&2
            exit 1
            ;;
    esac

    echo "${os}-${arch}"
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --version)
            VERSION="$2"
            shift 2
            ;;
        --rid)
            RUNTIME_ID="$2"
            shift 2
            ;;
        --output-dir)
            OUTPUT_DIR="$2"
            shift 2
            ;;
        --help|-h)
            usage
            exit 0
            ;;
        *)
            echo "error: unknown argument '$1'" >&2
            usage >&2
            exit 1
            ;;
    esac
done

if [[ -z "$VERSION" ]]; then
    echo "error: --version is required" >&2
    usage >&2
    exit 1
fi

if [[ -z "$RUNTIME_ID" ]]; then
    RUNTIME_ID="$(detect_rid)"
fi

if [[ -z "$OUTPUT_DIR" ]]; then
    OUTPUT_DIR="$(pwd)/artifacts/local-release"
fi

PROJECT_PATH="src/grok-search-cli/grok-search-cli.csproj"
PUBLISH_DIR="$(mktemp -d /tmp/grok-search-cli-local-package.XXXXXX)"
ARCHIVE_NAME="grok-search-cli_${VERSION}_${RUNTIME_ID}.tar.gz"
CHECKSUM_NAME="grok-search-cli_${VERSION}_${RUNTIME_ID}.sha256"
COMBINED_CHECKSUM_NAME="checksums_${VERSION}.txt"

cleanup() {
    rm -rf "$PUBLISH_DIR"
}
trap cleanup EXIT

mkdir -p "$OUTPUT_DIR"

echo "Publishing grok-search-cli for ${RUNTIME_ID} ..."
dotnet publish "$PROJECT_PATH" \
    --configuration Release \
    --runtime "$RUNTIME_ID" \
    --self-contained \
    -p:PublishAot=true \
    -p:StripSymbols=true \
    -p:DebugType=None \
    -o "$PUBLISH_DIR"

if [[ ! -f "$PUBLISH_DIR/grok-search-cli" ]]; then
    echo "error: published binary not found in $PUBLISH_DIR" >&2
    exit 1
fi

tar czf "$OUTPUT_DIR/$ARCHIVE_NAME" -C "$PUBLISH_DIR" grok-search-cli
sha256sum "$OUTPUT_DIR/$ARCHIVE_NAME" > "$OUTPUT_DIR/$CHECKSUM_NAME"
cp "$OUTPUT_DIR/$CHECKSUM_NAME" "$OUTPUT_DIR/$COMBINED_CHECKSUM_NAME"

echo "Created archive: $OUTPUT_DIR/$ARCHIVE_NAME"
echo "Created checksum: $OUTPUT_DIR/$CHECKSUM_NAME"
echo "Updated combined checksums: $OUTPUT_DIR/$COMBINED_CHECKSUM_NAME"