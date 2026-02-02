package main

import (
	"fmt"
	"image/jpeg"
	"log"
	"os"
	"path/filepath"

	"github.com/nfnt/resize"
)

var supportedExt = map[string]bool{
	".jpg":  true,
	".jpeg": true,
}

func main() {
	if len(os.Args) < 2 || !supportedExt[filepath.Ext(os.Args[1])] {
		fmt.Println("Error: jpg image path argument required.")
		return
	}

	fmt.Println("----------------------------------------")
	fmt.Printf("Converting the following files:\n%s\n", os.Args[1:])
	fmt.Println("----------------------------------------")

	for _, f := range os.Args[1:] {
		ShrinkImage(f)
	}
}

func ShrinkImage(inPath string) {

	file, err := os.Open(inPath)
	if err != nil {
		log.Fatalf(err.Error())
	}
	defer file.Close()

	// decode jpeg into image.Image
	img, err := jpeg.Decode(file)
	if err != nil {
		log.Fatal(err)
	}

	// resize to width 1500 using Lanczos resampling
	m := resize.Resize(1500, 0, img, resize.Lanczos3)

	out, err := os.Create(filepath.Join(filepath.Dir(inPath), "_"+filepath.Base(inPath)))
	if err != nil {
		log.Fatalf("error creating file: %s", err)
	}
	defer out.Close()

	// write new image to file
	quality := &jpeg.Options{Quality: 50}
	err = jpeg.Encode(out, m, quality)
	if err != nil {
		log.Fatalf(err.Error())
	}
}
