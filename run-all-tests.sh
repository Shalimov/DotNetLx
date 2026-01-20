#!/bin/bash

# Script to run all .lx test files and collect output
# Usage: ./run-all-tests.sh [output-file]

# Default output file
OUTPUT_FILE="${1:-test-results.txt}"

# Colors for terminal output
GREEN='\033[0;32m'
RED='\033[0;31m'
BLUE='\033[0;34m'
YELLOW='\033[0;33m'
NC='\033[0m' # No Color

# Get the directory where the script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/DotNetLxInterpreter"
SCRIPTS_DIR="$PROJECT_DIR/LxScripts"

# Convert OUTPUT_FILE to absolute path if it's relative
if [[ "$OUTPUT_FILE" != /* ]]; then
    OUTPUT_FILE="$SCRIPT_DIR/$OUTPUT_FILE"
fi

# Check if LxScripts directory exists
if [ ! -d "$SCRIPTS_DIR" ]; then
    echo -e "${RED}Error: LxScripts directory not found at $SCRIPTS_DIR${NC}"
    exit 1
fi

# Function to determine if a test is expected to fail based on filename
is_expected_to_fail() {
    local filename="$1"

    # Tests expected to fail (error tests and invalid cases)
    if [[ "$filename" == *"-error.lx" ]] || \
       [[ "$filename" == "unused-var-"* ]] || \
       [[ "$filename" == "break-outside"* ]] || \
       [[ "$filename" == "func-too-"* ]] || \
       [[ "$filename" == "declaration-self-ref.lx" ]] || \
       [[ "$filename" == "non-init-var-runtime.lx" ]] || \
       [[ "$filename" == "self-init-"*"-error.lx" ]] || \
       [[ "$filename" == "function-inside-loop-with-break.lx" ]] || \
       [[ "$filename" == "return-statement-toplevel-error.lx" ]] || \
       [[ "$filename" == "return-statement-in-block-error.lx" ]] || \
       [[ "$filename" == "self-init-expression-error.lx" ]] || \
       [[ "$filename" == "self-init-function-error.lx" ]] || \
       [[ "$filename" == "self-init-function-call-error.lx" ]] || \
       [[ "$filename" == "self-init-ternary-error.lx" ]]; then
         return 0  # true - expected to fail
     fi

     return 1  # false - expected to succeed
 }

# Clear the output file
> "$OUTPUT_FILE"

echo -e "${BLUE}=== Running Lx Test Files ===${NC}"
echo "Output will be saved to: $OUTPUT_FILE"
echo ""
echo -e "${YELLOW}Test Categories:${NC}"
echo "  • index-based-variable-access-test.lx     - New: Tests refactored Environment/EnvironmentValueKeeper"
echo "  • unused-var-*.lx                         - Detects unused variables in scopes"
echo "  • break-outside-loop-*.lx                 - Validates break statement placement"
echo "  • return-statement-*.lx                   - Validates return statement placement"
echo "  • self-init-*.lx                          - Detects self-referential variable initialization"
echo "  • lexical-scope-variable-extraction.lx    - Tests complex scope extraction"
echo "  • function-inside-*.lx                    - Tests functions within loops/assignments"
echo ""

# Counter for statistics
total_files=0
passed_as_expected=0
failed_as_expected=0
unexpected_pass=0
unexpected_fail=0

# Find all .lx files and sort them
while IFS= read -r file; do
    total_files=$((total_files + 1))
    filename=$(basename "$file")

    # Determine if test is expected to fail
    if is_expected_to_fail "$filename"; then
        expected_result="FAIL"
        echo -e "${BLUE}Running: $filename ${YELLOW}[EXPECTED: FAIL]${NC}"
    else
        expected_result="PASS"
        echo -e "${BLUE}Running: $filename ${GREEN}[EXPECTED: PASS]${NC}"
    fi

    # Write separator to output file
    echo "=================================" >> "$OUTPUT_FILE"
    echo "-- $filename Output --" >> "$OUTPUT_FILE"
    echo "-- EXPECTED: $expected_result" >> "$OUTPUT_FILE"
    echo "=================================" >> "$OUTPUT_FILE"
    echo "" >> "$OUTPUT_FILE"

    # Run the file and capture output
    cd "$PROJECT_DIR"
    if dotnet run "$file" >> "$OUTPUT_FILE" 2>&1; then
        actual_result="PASS"
        # Test passed - check if it was expected
        if [ "$expected_result" = "PASS" ]; then
            echo -e "${GREEN}✓ PASS (as expected)${NC}"
            passed_as_expected=$((passed_as_expected + 1))
        else
            echo -e "${RED}✗ UNEXPECTED PASS (expected to fail!)${NC}"
            unexpected_pass=$((unexpected_pass + 1))
        fi
    else
        actual_result="FAIL"
        # Test failed - check if it was expected
        if [ "$expected_result" = "FAIL" ]; then
            echo -e "${GREEN}✓ FAIL (as expected)${NC}"
            failed_as_expected=$((failed_as_expected + 1))
        else
            echo -e "${RED}✗ UNEXPECTED FAIL (expected to pass!)${NC}"
            unexpected_fail=$((unexpected_fail + 1))
        fi
    fi

    # Write result to output file
    echo "" >> "$OUTPUT_FILE"
    echo "-- ACTUAL: $actual_result" >> "$OUTPUT_FILE"
    echo "" >> "$OUTPUT_FILE"

done < <(find "$SCRIPTS_DIR" -name "*.lx" -type f | sort)

# Print summary
echo ""
echo -e "${BLUE}=== Test Summary ===${NC}"
echo "Total files: $total_files"
echo ""
echo -e "${GREEN}Passed as expected: $passed_as_expected${NC}"
echo -e "${GREEN}Failed as expected: $failed_as_expected${NC}"
if [ $unexpected_pass -gt 0 ] || [ $unexpected_fail -gt 0 ]; then
    echo ""
    echo -e "${YELLOW}=== Issues ===${NC}"
    if [ $unexpected_pass -gt 0 ]; then
        echo -e "${RED}Unexpected passes: $unexpected_pass${NC}"
    fi
    if [ $unexpected_fail -gt 0 ]; then
        echo -e "${RED}Unexpected failures: $unexpected_fail${NC}"
    fi
fi
echo ""
total_correct=$((passed_as_expected + failed_as_expected))
echo -e "${BLUE}Correct results: $total_correct / $total_files${NC}"
echo ""
echo "Full output saved to: $OUTPUT_FILE"
