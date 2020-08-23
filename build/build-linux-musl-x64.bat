@ECHO OFF
echo Building MUSL 64bit Linux...
cd ..
dotnet publish -r linux-musl-x64 -c Release /p:PublishSingleFile=true -p:PublishTrimmed=true
cd build
echo Build finished, you'll find the executable under "Garry's Mod Addon Merger/bin/Release/netcoreapp3/linux-musl-x64/publish"