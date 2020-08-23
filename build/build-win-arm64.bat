@ECHO OFF
echo Building 64bit ARM Windows...
cd ..
dotnet publish -r win-arm64 -c Release /p:PublishSingleFile=true -p:PublishTrimmed=true
cd build
echo Build finished, you'll find the executable under "Garry's Mod Addon Merger/bin/Release/netcoreapp3/win-arm64/publish"