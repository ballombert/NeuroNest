#!/usr/bin/env bash
##########################################################################
# This is the Cake bootstrapper script for Linux and OS X.
##########################################################################

# Define default arguments.
SCRIPT="build.cake"
CAKE_VERSION="4.0.0"
TARGET="Default"
CONFIGURATION="Release"
VERBOSITY="normal"
DRYRUN=false
SHOW_VERSION=false
SCRIPT_ARGUMENTS=()

# Parse arguments.
for i in "$@"; do
    case $1 in
        -s|--script) SCRIPT="$2"; shift ;;
        -t|--target) TARGET="$2"; shift ;;
        -c|--configuration) CONFIGURATION="$2"; shift ;;
        -v|--verbosity) VERBOSITY="$2"; shift ;;
        --version) SHOW_VERSION=true ;;
        --dryrun) DRYRUN=true ;;
        --) shift; SCRIPT_ARGUMENTS+=("$@"); break ;;
        *) SCRIPT_ARGUMENTS+=("$1") ;;
    esac
    shift
done

# Install Cake
echo "Installing Cake.Tool..."
dotnet tool restore
if [ $? -ne 0 ]; then
    dotnet tool install --global Cake.Tool --version "$CAKE_VERSION"
fi

# Build arguments
CAKE_ARGUMENTS=(
    "$SCRIPT"
    "--target=$TARGET"
    "--configuration=$CONFIGURATION"
    "--verbosity=$VERBOSITY"
)

if $DRYRUN; then
    CAKE_ARGUMENTS+=(--dryrun)
fi

CAKE_ARGUMENTS+=("${SCRIPT_ARGUMENTS[@]}")

# Start Cake
echo "Running build script..."
dotnet cake "${CAKE_ARGUMENTS[@]}"
exit $?
