#!/bin/bash

# Script to run all .lx test files and collect output
# Usage: ./run-all-tests.sh [output-file]

# Default output file
OUTPUT_FILE="${1:-test-results.txt}"

# Colors for terminal output
GREEN='\033[0;32m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Get the directory where the script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/DotNetLoxInterpreter"
SCRIPTS_DIR="$PROJECT_DIR/LoxScripts"

# Check if LoxScripts directory exists
if [ ! -d "$SCRIPTS_DIR" ]; then
    echo -e "${RED}Error: LoxScripts directory not found at $SCRIPTS_DIR${NC}"
    exit 1
fi

# Clear the output file
> "$OUTPUT_FILE"

echo -e "${BLUE}=== Running Lox Test Files ===${NC}"
echo "Output will be saved to: $OUTPUT_FILE"
echo ""

# Counter for statistics
total_files=0
successful_files=0
failed_files=0

# Find all .lx files and sort them
while IFS= read -r file; do
    total_files=$((total_files + 1))
    filename=$(basename "$file")

    echo -e "${BLUE}Running: $filename${NC}"

    # Write separator to output file
    echo "=================================" >> "$OUTPUT_FILE"
    echo "-- $filename Output --" >> "$OUTPUT_FILE"
    echo "=================================" >> "$OUTPUT_FILE"
    echo "" >> "$OUTPUT_FILE"

    # Run the file and capture output
    cd "$PROJECT_DIR"
    if dotnet run "$file" >> "$OUTPUT_FILE" 2>&1; then
        echo -e "${GREEN}✓ Success${NC}"
        successful_files=$((successful_files + 1))
    else
        echo -e "${RED}✗ Failed (expected for error test files)${NC}"
        failed_files=$((failed_files + 1))
    fi

    # Add blank lines after each test
    echo "" >> "$OUTPUT_FILE"
    echo "" >> "$OUTPUT_FILE"

done < <(find "$SCRIPTS_DIR" -name "*.lx" -type f | sort)

# Print summary
echo ""
echo -e "${BLUE}=== Test Summary ===${NC}"
echo "Total files: $total_files"
echo -e "${GREEN}Successful: $successful_files${NC}"
echo -e "${RED}Failed: $failed_files${NC}"
echo ""
echo "Full output saved to: $OUTPUT_FILE"
