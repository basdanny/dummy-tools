# Dummy image shrink
Simple console that will shrink images by reducing resolution and quality of the images.

### Build
```
go build -ldflags "-s -w"
```
(flags to reduce output size)

To update the icon of the built exe:
```
go install -v github.com/akavel/rsrc@latest
rsrc -ico ./../icon.ico  
```

### Use
Run in terminal or drag and drop images on it.
