@ECHO OFF
echo Building 32bit ARM Windows...
cd ..
dotnet publish -r win-arm -c Release /p:PublishSingleFile=true -p:PublishTrimmed=true
cd build
echo Build finished, you'll find the executable under "Garry's Mod Addon Merger/bin/Release/netcoreapp3/win-arm/publish"