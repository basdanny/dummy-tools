# PDF Merge Desktop App

A small app that allows to merge multiple PDF files and images into a single PDF document.


## Build (.NET)
Run from terminal,  
For debug:  
```powershell
dotnet build
```  
For release (publish):  
```powershell
dotnet publish -c Release -r win-x64 -o:out/
```  

or with additional flags...  
```powershell
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained false -o:out/
```

#### Prerequisites
.NET 8.x
