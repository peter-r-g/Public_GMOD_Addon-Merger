# Garry's Mod Addon Merger
An easy to use text based tool with a more advanced interaction for command-line users. What this tool does is you can take a folder full of other folders (like a Garry's Mod dedicated servers addons folder) and merge them all under a single folder. Useful if you'd like to compress all of your addons to a single folder or you'd like to eliminate any duplicate files within your server structure.

## Getting Started
### Prerequisites
* [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)
* [Visual Studio 20XX](https://visualstudio.microsoft.com/vs/) (Only if you plan to contribute, solution has only been tested in VS 2019)
### Installing (For Users)
1. See the [releases page](https://github.com/tamewater/Public_GMOD_Addon-Merger/releases).
### Installing (For Developers)
1. Clone this repository
2. Open the solution file within the root of the repository.
3. Done! No extra setup should be necessary.
### Building (For Developers, Windows only)
Use any of the batch files within the build directory. There is also a shortcut to a list of supported runtime environments in case you want to built for something that isn't included.

## Supported Platforms
### Windows
* 32bit | Windows 7 - Windows 10 (Tested 2020-08-23)
* 64bit | Windows 7 - Windows 10 (Tested 2020-08-23)
* 32bit Arm | Windows 8 - Windows 10 (Not Tested)
* 64bit Arm | Windows 10 (Not Tested)
### macOS
* 64bit | Minimum of macOS 10.12 Sierra (Not Tested)
### Linux
* 64bit | CentOS, Debian, Fedora, Ubuntu, and derivatives (Not Tested)
* 64bit MUSL | Alpine Linux (Not Tested)
* 32bit Arm | Raspian on Raspberry Pi Model 2+ (Not Tested)
* 64bit Arm | Ubuntu Server on Raspberry Pi Model 3+ (Not Tested)

## Built With
* [.NET Core](https://dotnet.microsoft.com/download/dotnet-core)
* [Newtonsoft.Json](https://www.newtonsoft.com/json)

## Contributing
See [CONTRIBUTING.md](CONTRIBUTING.md)

## License
This project is licensed under the [GNU GPL v3.0](https://choosealicense.com/licenses/gpl-3.0/) license.
