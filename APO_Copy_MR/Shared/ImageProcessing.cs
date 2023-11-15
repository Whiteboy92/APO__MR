using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using ScottPlot;
using Parallel = System.Threading.Tasks.Parallel;
namespace APO_Copy_MR.Shared;

public static class ImageProcessing
{
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public static void Histogram(Image<Bgr, byte>? imageInput)
    {
        using var imageToProcess = imageInput?.Convert<Gray, byte>().Clone();
        if (imageToProcess == null)
        {
            return;
        }

        // Generate data for the histogram
        var histogramData = new int[256];

        Parallel.For(0, imageToProcess.Height, i =>
        {
            for (var j = 0; j < imageToProcess.Width; j++)
            {
                var intensity = (byte)imageToProcess[i, j].Intensity;
                Interlocked.Increment(ref histogramData[intensity]);
            }
        });

        // Convert histogram data to double[] for WpfPlot
        var histogramDataDouble = histogramData.Select(x => (double)x).ToArray();

        // Close the existing histogram window if it's open
        if (ImageWindow.ActiveHistogramWindow != null)
        {
            ImageWindow.ActiveHistogramWindow.Close();
            ImageWindow.ActiveHistogramWindow = null;
        }

        var histogramPlot = new WpfPlot();
        histogramPlot.Refresh();
        histogramPlot.Plot.AddBar(histogramDataDouble);

        var histogramWindow = new HistogramWindow
        {
            Width = 600,
            Height = 400,
            Content = histogramPlot,
            Title = $"Histogram of {ImageWindow.ActiveHistogramWindow?.Title}"
        };

        histogramPlot.MouseMove += (_, eMouseMove) =>
        {
            var mousePos = eMouseMove.GetPosition(histogramPlot);
            var mouseX = histogramPlot.Plot.GetCoordinateX((float)mousePos.X);
            var xValue = (long)Math.Round(mouseX);

            if (xValue is < 0 or >= 256) return;
            var yValue = histogramData[xValue];
            histogramWindow.Title = $"Value: {yValue}";
        };

        ImageWindow.ActiveHistogramWindow = histogramWindow;
        histogramWindow.Show();
    }
    
    public static Image<Bgr, byte>? HistogramStretchWithRange(Image<Bgr, byte>? inputImage, double minRange, double maxRange)
    {
        if (inputImage == null) { return null; }

        var outputImage = inputImage.Clone();
        inputImage.MinMax(out var minVal, out var maxVal, out _, out _);

        var scaleFactor = (maxRange - minRange) / (maxVal[0] - minVal[0]);
        var offset = minRange - scaleFactor * minVal[0];

        for (var y = 0; y < inputImage.Rows; y++)
        {
            for (var x = 0; x < inputImage.Cols; x++)
            {
                var currentPixel = inputImage[y, x];
                var newPixel = new Bgr
                {
                    Blue = Math.Min(Math.Max(currentPixel.Blue * scaleFactor + offset, minRange), maxRange),
                    Green = Math.Min(Math.Max(currentPixel.Green * scaleFactor + offset, minRange), maxRange),
                    Red = Math.Min(Math.Max(currentPixel.Red * scaleFactor + offset, minRange), maxRange)
                };

                outputImage[y, x] = newPixel;
            }
        }

        return outputImage;
    }

    public static List<Mat>? SplitChannels(Image<Bgr, byte>? imageInput)
    {
        if (imageInput == null) return null;
        var imageOutputHsv = imageInput.Convert<Hsv, byte>();

        var vectorOfMats = new VectorOfMat();
        CvInvoke.Split(imageOutputHsv, vectorOfMats);

        var mats = new List<Mat>();
        for (var i = 0; i < vectorOfMats.Size; i++)
        {
            mats.Add(vectorOfMats[i]);
        }

        return mats;
    }

    public static List<Mat>? ConvertToLab(Image<Bgr, byte>? imageInput)
    {
        if (imageInput == null) return null;
        var imageOutputLab = imageInput.Convert<Lab, byte>();

        // Split the Lab image into its channels
        var channels = new VectorOfMat();
        CvInvoke.Split(imageOutputLab, channels);

        // Convert the VectorOfMat to a list of Mat
        var mats = new List<Mat>();
        for (var i = 0; i < channels.Size; i++)
        {
            mats.Add(channels[i]);
        }
            
        return mats;
    }
    
    public static Image<Bgr, byte>? ConvertToGray(Image<Bgr, byte>? inputImage)
    {
        var imageOutputGray = inputImage?.Convert<Gray, byte>();
        return imageOutputGray?.Convert<Bgr, byte>();
    }
    
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    [SuppressMessage("ReSharper.DPA", "DPA0001: Memory allocation issues")]
    public static Image<Bgr, byte>? Negate(Image<Bgr, byte>? inputImage)
    {
        if (inputImage == null) { return null; }

        using var imageToNegate = inputImage.Clone();
        var negatedImage = imageToNegate.CopyBlank();

        Parallel.For(0, imageToNegate.Height, y =>
        {
            for (var x = 0; x < imageToNegate.Width; x++)
            {
                var originalPixel = imageToNegate[y, x];
                var newBlue = (byte)(255 - originalPixel.Blue);
                var newGreen = (byte)(255 - originalPixel.Green);
                var newRed = (byte)(255 - originalPixel.Red);
                var negatedPixel = new Bgr(newBlue, newGreen, newRed);
                negatedImage[y, x] = negatedPixel;
            }
        });

        return negatedImage;
    }
    
    public static Image<Bgr, byte>? Posterize(Image<Bgr, byte>? inputImage, int colorLevels)
    {
        if (inputImage == null) { return null; }

        using var imageToAnalyze = inputImage.Clone();
        var grayscaleImage = imageToAnalyze.Convert<Gray, byte>();

        var quantizationFactor = 256.0 / colorLevels;

        var posterizedImage = grayscaleImage.CopyBlank();
        Parallel.For(0, grayscaleImage.Height, y =>
        {
            for (var x = 0; x < grayscaleImage.Width; x++)
            {
                var originalPixel = grayscaleImage[y, x].Intensity;
                var posterizedPixel = (byte)(Math.Floor(originalPixel / quantizationFactor) * quantizationFactor);
                posterizedImage[y, x] = new Gray(posterizedPixel);
            }
        });

        return posterizedImage.Convert<Bgr, byte>();
    }
    
    public static Image<Bgr, byte>? Erode(Image<Bgr, byte>? inputImage, int iterations)
    {
        if (inputImage == null) { return null; }

        using var imageToAnalyze = inputImage.Clone();
        var erodedImage = imageToAnalyze.Erode(iterations);
        return erodedImage;
    }
    
    public static Image<Bgr, byte>? Dilate(Image<Bgr, byte>? inputImage, int iterations)
    {
        if (inputImage == null) { return null; }

        using var imageToAnalyze = inputImage.Clone();
        var dilatedImage = imageToAnalyze.Dilate(iterations);
        return dilatedImage.Convert<Bgr, byte>();
    }
    
    public static Image<Bgr, byte>? MorphologyEx(Image<Bgr, byte>? inputImage, MorphOp morphOp, Size size, System.Drawing.Point anchor, int iterations)
    {
        if (inputImage == null) { return null; }

        using var imageToAnalyze = inputImage.Mat.Clone();
        var resultImage = new Mat();
        CvInvoke.MorphologyEx(imageToAnalyze, resultImage, morphOp,
            CvInvoke.GetStructuringElement(ElementShape.Rectangle, size, anchor),
            new System.Drawing.Point(-1, -1), iterations, BorderType.Default, new MCvScalar());

        return resultImage.ToImage<Bgr, byte>();
    }
    
    public static Image<Gray, byte>? Skeletonize(Image<Gray, byte>? inputImage)
    {
        if (inputImage == null) { return null; }

        var skeletonImage = inputImage.CopyBlank();
        var temp = new Image<Gray, byte>(inputImage.Width, inputImage.Height);
        var eroded = new Image<Gray, byte>(inputImage.Width, inputImage.Height);

        var element = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(3, 3), new System.Drawing.Point(-1, -1));

        bool done;
        do
        {
            CvInvoke.Erode(inputImage, eroded, element, new System.Drawing.Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            CvInvoke.Dilate(eroded, temp, element, new System.Drawing.Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            CvInvoke.Subtract(inputImage, temp, temp);
            CvInvoke.BitwiseOr(skeletonImage, temp, skeletonImage);
            eroded.CopyTo(inputImage);

            done = (CvInvoke.CountNonZero(inputImage) == 0);
        } while (!done);

        return skeletonImage;
    }
    
    public static Image<Bgr, byte> LinearSharpening(Image<Bgr, byte> image, int[] matrix)
    {
        // Ensure that the matrix has 9 elements
        if (matrix.Length != 9)
        {
            throw new ArgumentException("Invalid matrix size. The matrix should contain 9 elements.");
        }

        // Clone the input image to avoid modifying it directly
        var outputImage = image.Clone();

        // Create a 3x3 kernel with the matrix values
        var kernelValues = new float[3, 3];
        var index = 0;
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                kernelValues[i, j] = matrix[index];
                index++;
            }
        }

        // Create a kernel using the matrix values
        var kernel = new ConvolutionKernelF(kernelValues);

        // Apply the kernel to the image using the Filter2D method
        CvInvoke.Filter2D(outputImage, outputImage, kernel, new System.Drawing.Point(-1, -1));

        return outputImage;
    }

    public static Image<Bgr, byte> LinearSmoothing(Image<Bgr, byte> imageToBlur)
    {
        using var src = imageToBlur.Mat;
        using var dst = new Mat();
        // Apply the GaussianBlur function to perform linear smoothing
        CvInvoke.GaussianBlur(src, dst, new Size(3, 3), 0);

        var blurredImage = dst.ToImage<Bgr, byte>();

        return blurredImage;
    }
    
    public static Image<Gray, byte> DetectEdgesSobel(Image<Bgr, byte> image)
    {
        var grayImage = image.Convert<Gray, byte>();
        var edgesImage = grayImage.Clone();

        CvInvoke.Sobel(grayImage, edgesImage, DepthType.Cv8U, 1, 1);

        return edgesImage;
    }

    public static Image<Gray, byte> DetectEdgesLaplacian(Image<Bgr, byte> image)
    {
        var grayImage = image.Convert<Gray, byte>();
        var edgesImage = grayImage.Clone();

        CvInvoke.Laplacian(grayImage, edgesImage, DepthType.Cv8U);

        return edgesImage;
    }

    public static Image<Gray, byte> DetectEdgesCanny(Image<Bgr, byte> image)
    {
        var grayImage = image.Convert<Gray, byte>();
        var edgesImage = grayImage.Clone();

        CvInvoke.Canny(grayImage, edgesImage, 100, 200);

        return edgesImage;
    }
    
    public static Image<Gray, byte> PerformPrewittEdgeDetection(Image<Gray, byte> input, string direction)
    { 
        var gradientX = input.Sobel(1, 0, 3);
        var gradientY = input.Sobel(0, 1, 3);
        
        var prewittEdgeDetection = new Image<Gray, byte>(input.Width, input.Height);
        
        switch (direction)
        {
            case "Top-Left":
                Parallel.For(0, prewittEdgeDetection.Height, y =>
                {
                        for (short x = 0; x < prewittEdgeDetection.Width; x++)
                        {
                            prewittEdgeDetection.Data[y, x, 0] = (byte)Math.Sqrt(Math.Pow(gradientX.Data[y, x, 0], 2) + Math.Pow(gradientY.Data[y, x, 0], 2));
                        }
                });
                break;
                
            case "Top":
                Parallel.For(0, prewittEdgeDetection.Height, y =>
                {
                        for (short x = 0; x < prewittEdgeDetection.Width; x++)
                        {
                            prewittEdgeDetection.Data[y, x, 0] = (byte)gradientY.Data[y, x, 0];
                        }
                });
                break;
                
            case "Top-Right":
                Parallel.For(0, prewittEdgeDetection.Height, y =>
                {
                        for (short x = 0; x < prewittEdgeDetection.Width; x++)
                        {
                            prewittEdgeDetection.Data[y, x, 0] = (byte)Math.Sqrt(Math.Pow(gradientX.Data[y, x, 0], 2) + Math.Pow(-gradientY.Data[y, x, 0], 2));
                        }
                });
                break;
                
            case "Left":
                Parallel.For(0, prewittEdgeDetection.Height, y =>
                {
                        for (short x = 0; x < prewittEdgeDetection.Width; x++)
                        {
                            prewittEdgeDetection.Data[y, x, 0] = (byte)gradientX.Data[y, x, 0];
                        }
                });
                break;
                
            case "Right": 
                Parallel.For(0, prewittEdgeDetection.Height, y => 
                {
                        for (short x = 0; x < prewittEdgeDetection.Width; x++)
                        {
                            prewittEdgeDetection.Data[y, x, 0] = (byte)-gradientX.Data[y, x, 0];
                        }
                });
                break;
                
            case "Bottom-Left":
                Parallel.For(0, prewittEdgeDetection.Height, y =>
                {
                        for (short x = 0; x < prewittEdgeDetection.Width; x++)
                        {
                            prewittEdgeDetection.Data[y, x, 0] = (byte)Math.Sqrt(Math.Pow(-gradientX.Data[y, x, 0], 2) + Math.Pow(gradientY.Data[y, x, 0], 2));
                        }
                });
                break;
                
            case "Bottom":
                Parallel.For(0, prewittEdgeDetection.Height, y =>
                {
                        for (short x = 0; x < prewittEdgeDetection.Width; x++)
                        {
                            prewittEdgeDetection.Data[y, x, 0] = (byte)-gradientY.Data[y, x, 0];
                        }
                });
                break;
                
            case "Bottom-Right":
                Parallel.For(0, prewittEdgeDetection.Height, y =>
                {
                        for (short x = 0; x < prewittEdgeDetection.Width; x++)
                        {
                            prewittEdgeDetection.Data[y, x, 0] = (byte)Math.Sqrt(Math.Pow(-gradientX.Data[y, x, 0], 2) + Math.Pow(-gradientY.Data[y, x, 0], 2));
                        }
                });
                break;
        }
            
        return prewittEdgeDetection;
    }
}
