#!/bin/bash
echo "========================================"
echo "Building AutoClicker (Mono)..."
echo "========================================"

# Check for mcs (Mono C# compiler)
if ! command -v mcs &> /dev/null; then
    echo "ERROR: Mono C# compiler (mcs) not found!"
    echo "Please install Mono: sudo apt-get install mono-complete"
    exit 1
fi

# Compile
echo "Compiling..."
mcs -target:winexe -out:AutoClicker.exe -optimize+ AutoClicker.cs

if [ $? -eq 0 ]; then
    echo ""
    echo "========================================"
    echo "SUCCESS! AutoClicker.exe created!"
    echo "========================================"
    echo ""
    echo "Run with: mono AutoClicker.exe"
    echo "Note: This is primarily for Windows systems"
    echo ""
else
    echo ""
    echo "========================================"
    echo "Build FAILED!"
    echo "========================================"
    echo ""
fi
