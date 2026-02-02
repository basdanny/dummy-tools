using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Linq;

namespace DummyImageCompressor
{
  class ImageCompressor
  {

    #region Constants
    private const double DefaultImageDimensionWidth = 1920.0;
    private const double DefaultImageDimensionHeight = 1080.0;
    #endregion

    public void Compress(string[] args)
    {
      double targetImageWidth, targetImageHeight;
      if (GetTargetWidthAndHeight(ref args, out targetImageWidth, out targetImageHeight))
      {

        foreach (string inputFilename in args)
        {
          using (Image image = Image.Load(inputFilename))
          {
            string outputFilename = inputFilename.Insert(inputFilename.LastIndexOf('\\') + 1, "_");

            var outputImageDimensions = GetOutputImageDimensions(image, targetImageWidth, targetImageHeight);

            image.Mutate(x => x
               .Resize(outputImageDimensions.Width, outputImageDimensions.Height)
            );

            if (Path.GetExtension(inputFilename).ToLower() == ".jpg")
            {
              var jpgEncoderOptions = new JpegEncoder { Quality = 90};
              image.Save(outputFilename, jpgEncoderOptions);
            }
            else
            {
              image.Save(outputFilename);
            }

            Console.WriteLine("Picture resized: " + outputFilename);
          }
        }
      }
    }

    private bool GetTargetWidthAndHeight(ref string[] args, out double targetImageWidth, out double targetImageHeight)
    {
      targetImageWidth = DefaultImageDimensionWidth;
      targetImageHeight = DefaultImageDimensionHeight;

      foreach (string argument in args)
      {
        string[] splitted = argument.Split('=');

        if (argument.StartsWith("width="))
        {
          int result;
          if (int.TryParse(argument.Split('=')[1], out result))
            targetImageWidth = (double)result;
          else
          {
            Console.WriteLine("bad \"width\" parameter!");
            return false;
          }
        }
        if (argument.StartsWith("height="))
        {
          int result;
          if (int.TryParse(argument.Split('=')[1], out result))
            targetImageHeight = (double)result;
          else
          {
            Console.WriteLine("bad \"height\" parameter!");
            return false;
          }
        }
      }
      args = args.Where((arg) => !arg.StartsWith("width=") && !arg.StartsWith("height=")).ToArray();

      return true;
    }

    private Size GetOutputImageDimensions(Image inputImage, double targetImageWidth, double targetImageHeight)
    {
      double imageHeight = (double)inputImage.Height;
      double imageWidth = (double)inputImage.Width;
      if (imageWidth > targetImageWidth)
      {
        double ratio = targetImageWidth / imageWidth;
        imageWidth = targetImageWidth;
        imageHeight *= ratio;
      }
      if (imageHeight > targetImageHeight)
      {
        double ratio = targetImageHeight / imageHeight;
        imageHeight = targetImageHeight;
        imageWidth *= ratio;
      }

      return new Size(Convert.ToInt32(imageWidth), Convert.ToInt32(imageHeight));

    }
  }
}
