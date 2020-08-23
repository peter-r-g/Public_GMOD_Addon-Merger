@ECHO OFF
echo Building 32bit Windows...
cd ..
dotnet publish -r win-x86 -c Release /p:PublishSingleFile=true -p:PublishTrimmed=true
cd build
echo Build finished, you'll find the executable under "Garry's Mod Addon Merger/bin/Release/netcoreapp3/win-x86/publish"