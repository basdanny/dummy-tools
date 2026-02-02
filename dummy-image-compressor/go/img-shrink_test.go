package main

import (
	"os"
	"path/filepath"
	"testing"
)

func TestImageShrink(t *testing.T) {
	path, err := filepath.Abs("./fixtures/test.jpg")
	if err != nil {
		t.Errorf("No test input file found")
	}
	ShrinkImage(path)

	outPath := filepath.Join(filepath.Dir(path), "_"+filepath.Base(path))
	fi, err := os.Stat(outPath)
	if err != nil {
		t.Errorf("Output file %q not found. Error: %s", outPath, err)
	}

	if fi.Size() <= 0 {
		t.Errorf("Output file %q is empty", outPath)
	}

	t.Cleanup(func() { os.Remove(outPath) })
}
