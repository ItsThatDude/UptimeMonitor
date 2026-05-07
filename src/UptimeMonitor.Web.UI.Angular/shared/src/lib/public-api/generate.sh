#!/bin/bash
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd)"

swagger-typescript-api generate -p "$SCRIPT_DIR/swagger.json" -o "$SCRIPT_DIR" -n api-client.service.ts --module-name-index 1