using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageAnalysis
{
    internal class ImageReader
    {


        public static void Readimage()
        {

            var path = "C:\\Users\\Alex\\source\\repos\\ImageAnalysis\\ImageAnalysis\\Images\\Auto.jpg";
            var relative = @"Images\Auto.jpg";

            var file = File.ReadAllBytes(path);

            Console.WriteLine(file.Length);
            Bitmap bitmap = new Bitmap(path);

            Color pixelColor = bitmap.GetPixel(120, 140);

            Console.WriteLine(pixelColor.ToString());

            


            double[,] xSobel = new double[,]
            {
                {-1, 0, 1 },
                {-2, 0, 2},
                {-1, 0, 1 }
            };

            double[,] ySobel = new double[,]
            {
                {1, 2, 1 },
                {0, 0, 0},
                {-1, -2, -1 }
            };

            var width = bitmap.Width;
            var height = bitmap.Height;
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int bytes = bitmapData.Stride * bitmap.Height;
            byte[] pixelBuffer = new byte[bytes];


            // GEt the address of the first pixel data
            IntPtr srcScan0 = bitmapData.Scan0;

            // Copy image data to one of the byte arrays
            Marshal.Copy(srcScan0, pixelBuffer, 0, bytes);
            // unlock bits from system memory
            bitmap.UnlockBits(bitmapData);


            float rgb = 0;
            for(int i = 0; i < pixelBuffer.Length; i+= 4)
            {
                rgb = pixelBuffer[i] * .21f;
                rgb += pixelBuffer[i + 1] * .72f;
                rgb += pixelBuffer[i + 2] * .071f;

                pixelBuffer[i] = (byte)rgb;
                pixelBuffer[i + 1] = pixelBuffer[i];
                pixelBuffer[i + 2] = pixelBuffer[i];
                pixelBuffer[i + 3] = 255;
            }

            // Create variable for pixel data for each kernel
            double xg = 0.0;
            double yg = 0.0;
            double gt = 0.0;

            // This is how much our center pixel is offset from the border of our kernel
            // Sobel is 3x3, so center is 1 pixel from the kernel border
            int filterOffset = 1;
            int calcOffset = 0;
            int byteOffset = 0;

            byte[] resultBuffer = new byte[bytes];
            
            // Start with the pixel that is offset 1 from top and 1 from left side
            for(int offsetY = filterOffset; offsetY < height - filterOffset; offsetY++)
            {
                for(int offsetX = filterOffset; offsetX < width - filterOffset; offsetX++)
                {
                    // reset rgb values to 0
                    xg = yg = 0;
                    gt = 0.0;

                    // position of the kernel center pixel
                    byteOffset = offsetY * bitmapData.Stride + offsetX * 4;

                    // kernel calculations
                    for(int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for(int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + filterX * 4 + filterY * bitmapData.Stride;

                            xg += (double)(pixelBuffer[calcOffset + 1]) * xSobel[filterY + filterOffset, filterX + filterOffset];
                            yg += (double)(pixelBuffer[calcOffset + 1]) * ySobel[filterY + filterOffset, filterX + filterOffset];
                        }
                    }

                    // total rbg values for this pixel 
                    gt = Math.Sqrt((xg * xg) + (yg * yg));
                    if (gt > 255) gt = 255;
                    else if(gt < 0) gt = 0;

                    //set new data in the other byte array for output image data

                    resultBuffer[byteOffset] = (byte)(gt);
                    resultBuffer[byteOffset + 1] = (byte)(gt);
                    resultBuffer[byteOffset + 2] = (byte)(gt);
                    resultBuffer[byteOffset + 3] = 255;

                }
            }

            // Create new Bitmap which will hold the processed data
            Bitmap resultImage = new Bitmap(width, height);
            
            // Lock bits into system memory
            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            // Copy from byte arrays that holds processed data to bitmap
            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);

            // unlock bits from system memory 
            resultImage.UnlockBits(resultData);

            var newpath = "C:\\Users\\Alex\\source\\repos\\ImageAnalysis\\ImageAnalysis\\Images\\test.jpg";

            resultImage.Save(newpath, System.Drawing.Imaging.ImageFormat.Jpeg);


        }


        private static void Approximation()
        {
            
        }
        
    }
}
