@ECHO OFF
echo Building all versions...

echo Windows...
CALL build-win-arm
CALL build-win-arm64
CALL build-win-x64
CALL build-win-x86

echo MacOS...
CALL build-osx-x64

echo Linux...
CALL build-linux-x64
CALL build-linux-musl-x64
CALL build-linux-arm
CALL build-linux-arm64

echo Done!
pause