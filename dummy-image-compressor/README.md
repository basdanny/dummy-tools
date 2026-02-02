# DummyImageCompressor
The following utility compresses (jpg) image files in order to reduce size. E.g., to send images in the emails.

Drop jpeg files on the "DummyImageCompressor.exe". New compressed files will be placed in the original directory having '_' sign as a prefix.           
Default image dimensions are 1920x1080 . For custom dimensions size, use the following parameters (one or both) when calling the app: `width=_here_goes_the_value_` `height=_here_goes_the_value_`
            

## Build

### .NET Framework
Build the solution (in VS or MSBuild).

##### Prerequisites
.NET 4.8.x



### .NET (Core)
run from terminal:  
```powershell
dotnet build
```  
or for release:  
```powershell
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained false
```

#### Prerequisites
.NET 8.x


## Golang
Go [here](./go/README.md).
