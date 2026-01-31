# PerfReg Makefile

.PHONY: all build clean run test help install publish

# Default target
all: build

# Build the project
build:
	dotnet build

# Clean build artifacts
clean:
	dotnet clean
	rm -rf bin/ obj/
	rm -f *.benchmark.json

# Run PerfReg (requires ARGS variable)
# Usage: make run ARGS="run MyApp.exe"
run:
	dotnet run -- $(ARGS)

# Run help menu
help-menu:
	dotnet run -- help

# Test with the TestProgram
test: build
	dotnet run -- run ../TestProgram/bin/Debug/net8.0/TestProgram.exe 1
	dotnet run -- run ../TestProgram/bin/Debug/net8.0/TestProgram.exe 2
	dotnet run -- history

# Install as a global tool (publish first)
install: publish
	dotnet tool install --global --add-source ./nupkg perfreg

# Publish as a standalone executable
publish:
	dotnet publish -c Release -o ./publish

# Watch mode for development
watch:
	dotnet watch run

# Restore dependencies
restore:
	dotnet restore

# Format code
format:
	dotnet format

# Show version
version:
	dotnet run -- version

# Clear all benchmark history
clear:
	dotnet run -- clear