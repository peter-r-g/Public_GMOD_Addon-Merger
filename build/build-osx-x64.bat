@ECHO OFF
echo Building macOS...
cd ..
dotnet publish -r osx-x64 -c Release /p:PublishSingleFile=true -p:PublishTrimmed=true
cd build
echo Build finished, you'll find the executable under "Garry's Mod Addon Merger/bin/Release/netcoreapp3/osx-x64/publish"