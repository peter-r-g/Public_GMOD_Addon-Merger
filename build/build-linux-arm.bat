@ECHO OFF
echo Building 32bit ARM Linux...
cd ..
dotnet publish -r linux-arm -c Release /p:PublishSingleFile=true -p:PublishTrimmed=true
cd build
echo Build finished, you'll find the executable under "Garry's Mod Addon Merger/bin/Release/netcoreapp3/linux-arm/publish"