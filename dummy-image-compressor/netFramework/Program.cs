using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyImageCompressor
{
    internal class Program
    {

        #region Constants
        private const double DefaultImageDimensionWidth = 1920.0;
        private const double DefaultImageDimensionHeight = 1080.0;
        #endregion


        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("This utility compresses JPEG image files supplied as parameters.");
                Console.WriteLine("New compressed files will be placed in the original directory having '_' sign as a prefix.");
                Console.WriteLine("Default image dimensions are 1920x1080 . For custom dimensions size, use the following parameters (one or both) when calling the app: width=_here_goes_the_value_ height=_here_goes_the_value_ ");
            }

            double targetImageWidth, targetImageHeight;
            if (GetTargetWidthAndHeight(ref args, out targetImageWidth, out targetImageHeight))
            {

                foreach (string inputFilename in args)
                {
                    Image inputImage = (Image)null;
                    try
                    {
                        inputImage = Image.FromFile(inputFilename);
                        string outputFilename = inputFilename.Insert(inputFilename.LastIndexOf('\\') + 1, "_");
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

                        using (Image outputImage = (Image)new Bitmap((int)imageWidth, (int)imageHeight))
                        {
                            using (Graphics graphics = Graphics.FromImage(outputImage))
                            {
                                graphics.DrawImage(inputImage, 0, 0, (int)imageWidth, (int)imageHeight);
                            }
                            using (EncoderParameters encoderParams = new EncoderParameters(1))
                            {
                                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);
                                ImageCodecInfo encoder = ImageCodecInfo.GetImageEncoders()[1];
                                outputImage.Save(outputFilename, encoder, encoderParams);
                            }
                        }

                        Console.WriteLine("Picture resized: " + outputFilename);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Picture resize FAILED!!! " + inputFilename);
                        Console.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        if (inputImage != null)
                            inputImage.Dispose();
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("********************************************************");
            Console.WriteLine("Press any key to close.");
            Console.ReadKey();
        }

        static bool GetTargetWidthAndHeight(ref string[] args, out double targetImageWidth, out double targetImageHeight)
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

    }
}
