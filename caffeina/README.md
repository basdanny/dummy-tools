# Caffeina ☕

**Keep Your Computer Awake**

Caffeina is a Windows console application that prevents Windows computers from going to sleep, locking, or entering power-save mode while running.

## Build (.NET)
Run from terminal,  
For debug:  
```powershell
dotnet build
```  

For release (publish): 
```powershell
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained false -o:out\
```

#### Prerequisites
.NET 8.x
