@ECHO OFF
echo Building 64bit Windows...
cd ..
dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true -p:PublishTrimmed=true
cd build
echo Build finished, you'll find the executable under "Garry's Mod Addon Merger/bin/Release/netcoreapp3/win-x64/publish"