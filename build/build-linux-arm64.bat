@ECHO OFF
echo Building 64bit ARM Linux...
cd ..
dotnet publish -r linux-arm64 -c Release /p:PublishSingleFile=true -p:PublishTrimmed=true
cd build
echo Build finished, you'll find the executable under "Garry's Mod Addon Merger/bin/Release/netcoreapp3/linux-arm64/publish"