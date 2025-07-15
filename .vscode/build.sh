#!/bin/bash
set -e

export RimWorldVersion="$1"
CONFIGURATION="Debug"
TARGET="$HOME/.var/app/com.valvesoftware.Steam/.local/share/Steam/steamapps/common/RimWorld/Mods/NeuralSuperchargerOwnership"

# build dlls
dotnet build --configuration "$CONFIGURATION" Source/NeuralSuperchargerOwnership/NeuralSuperchargerOwnership.csproj

# remove mod folder
rm -rf "$TARGET"

# copy mod files
mkdir -p "$TARGET/$RimWorldVersion"
cp -r $RimWorldVersion/* "$TARGET/$RimWorldVersion/"

# copy interop mod files
# <NONE>

mkdir -p "$TARGET/Common"
cp -r Common/* "$TARGET/Common/"

mkdir -p "$TARGET/About"
cp About/About.xml "$TARGET/About/"
cp About/Preview.png "$TARGET/About/"
cp About/ModIcon.png "$TARGET/About/"
cp About/PublishedFileId.txt "$TARGET/About/"

cp CHANGELOG.md "$TARGET/"
cp LICENSE "$TARGET/"
cp LICENSE.Apache-2.0 "$TARGET/"
cp LICENSE.MIT "$TARGET/"
cp README.md "$TARGET/"
#cp LoadFolders.xml "$TARGET/"

# Trigger auto-hotswap
mkdir -p "$TARGET/$RimWorldVersion/Assemblies"
touch "$TARGET/$RimWorldVersion/Assemblies/NeuralSuperchargerOwnership.dll.hotswap"
