@ECHO OFF
echo Building 64bit Linux...
cd ..
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true -p:PublishTrimmed=true
cd build
echo Build finished, you'll find the executable under "Garry's Mod Addon Merger/bin/Release/netcoreapp3/linux-x64/publish"