using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kyoto
{
    class Filters
    {
        public static Bitmap Gaussiano(Bitmap srcImage)
        {
            //OBTENER MATRIZ KERNEL
            int lenght = 5;         //Tamaño de matriz 5x5
            double weight = 5.5;    //Ponderacion de filtro
            double[,] kernel = new double[lenght, lenght];
            double kernelSum = 0;
            int foff = (lenght - 1) / 2;
            double distance = 0;
            //Operacion de filtro
            double constant = 1d / (2 * Math.PI * weight * weight);
            for (int y = -foff; y <= foff; y++)
            {
                for (int x = -foff; x <= foff; x++)
                {
                    //Calcular distancia del centro
                    distance = ((y * y) + (x * x)) / (2 * weight * weight);
                    //Guardar valor de distancia en la matriz
                    kernel[y + foff, x + foff] = constant * Math.Exp(-distance);
                    //Sumar el valor para calcularlo
                    kernelSum += kernel[y + foff, x + foff];
                }
            }
            //Asignar valores finales de la matriz
            for (int y = 0; y < lenght; y++)
            {
                for (int x = 0; x < lenght; x++)
                {
                    //Dividir valor entre la suma de todos los valores del kernel
                    kernel[y, x] = kernel[y, x] * 1d / kernelSum;
                }
            }


            //CALCULAR GAUSSIANO
            int width = srcImage.Width;
            int height = srcImage.Height;
            BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int bytes = srcData.Stride * srcData.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(srcData.Scan0, buffer, 0, bytes);
            srcImage.UnlockBits(srcData);
            int colorChannels = 3;
            double[] rgb = new double[colorChannels];

            int kcenter = 0;
            int kpixel = 0;
            for (int y = foff; y < height - foff; y++)
            {
                for (int x = foff; x < width - foff; x++)
                {
                    for (int c = 0; c < colorChannels; c++)
                    {
                        rgb[c] = 0.0;
                    }
                    kcenter = y * srcData.Stride + x * 4;
                    for (int fy = -foff; fy <= foff; fy++)
                    {
                        for (int fx = -foff; fx <= foff; fx++)
                        {
                            kpixel = kcenter + fy * srcData.Stride + fx * 4;
                            for (int c = 0; c < colorChannels; c++)
                            {
                                rgb[c] += (double)(buffer[kpixel + c]) * kernel[fy + foff, fx + foff];
                            }
                        }
                    }
                    for (int c = 0; c < colorChannels; c++)
                    {
                        if (rgb[c] > 255)
                        {
                            rgb[c] = 255;
                        }
                        else if (rgb[c] < 0)
                        {
                            rgb[c] = 0;
                        }
                    }
                    for (int c = 0; c < colorChannels; c++)
                    {
                        result[kcenter + c] = (byte)rgb[c];
                    }
                    result[kcenter + 3] = 255;
                }
            }
            Bitmap resultImage = new Bitmap(width, height);
            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(result, 0, resultData.Scan0, bytes);
            resultImage.UnlockBits(resultData);
            return resultImage;
        }

        static public Bitmap SalYPimienta(Bitmap srcImage)
        {
            int r, g, b, a;
            int valorRuido = 50;
            int valorRandom;
            Bitmap result = new Bitmap(srcImage.Width, srcImage.Height);

            Random rnd = new Random();
            for (int i = 0; i < srcImage.Width - 1; i++)
            {
                for (int j = 0; j < srcImage.Height - 1; j++)
                {
                    //Asignar valores a rgb
                    valorRandom = rnd.Next(-(valorRuido - 1), valorRuido + 1);
                    r = srcImage.GetPixel(i, j).R + valorRandom;

                    //valorRandom = rnd.Next(-(valorRuido - 1), valorRuido + 1);
                    g = srcImage.GetPixel(i, j).G + valorRandom;

                    //valorRandom = rnd.Next(-(valorRuido - 1), valorRuido + 1);
                    b = srcImage.GetPixel(i, j).B + valorRandom;

                    //Alfa queda igual
                    a = srcImage.GetPixel(i, j).A;

                    if (r < 0)
                        r = 0;
                    if (r > 255)
                        r = 255;
                    if (g < 0)
                        g = 0;
                    if (g > 255)
                        g = 255;
                    if (b < 0)
                        b = 0;
                    if (b > 255)
                        b = 255;

                    //Pintar imagen resultante
                    result.SetPixel(i, j, Color.FromArgb(a, r, g, b));

                }
            }

            return result;
        }

        static public Bitmap Enfocar(Bitmap sourceImage)
        {
            int width = sourceImage.Width;
            int height = sourceImage.Height;

            //Bloquear los bits de la imagen de origen en la memoria del sistema
            BitmapData srcData = sourceImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            //Obtenga el número total de bytes en su imagen
            //- 32 bytes por píxel x ancho de imagen x alto de imagen -> para imágenes de 32 bpp
            int bytes = srcData.Stride * srcData.Height;

            //Matrices de bytes para contener la información de píxeles de su imagen
            byte[] pixelBuffer = new byte[bytes];
            byte[] resultBuffer = new byte[bytes];

            //Obtener la dirección de los primeros datos de píxeles
            IntPtr srcScan0 = srcData.Scan0;

            //Copiar los datos de la imagen en una de las matrices de bytes
            Marshal.Copy(srcScan0, pixelBuffer, 0, bytes);

            //Desbloquee bits de la memoria del sistema -> tenemos toda la información necesaria en la matriz
            sourceImage.UnlockBits(srcData);


            ///////////////////////////////     KERNEL      //////////////////////
            double[,] kernel =  {
                { 0, -1, 0 },
                { -1, 5, -1 },
                { 0, -1, 0}
            };

            //Crear una variable para datos de píxeles para cada elemento del kernel
            double xr = 0.0;
            double xg = 0.0;
            double xb = 0.0;
            double rt = 0.0;
            double gt = 0.0;
            double bt = 0.0;

            //Esto es cuánto se desplaza su píxel central del borde de su kernel
            //Sobel es 3x3, por lo que el centro está a 1 píxel del borde del kernel
            int filterOffset = 1;
            int calcOffset = 0;
            int byteOffset = 0;


            //Comience con el píxel que está desplazado 1 desde la parte superior y 1 desde el lado izquierdo
            //esto es tan kernel completo en tu imagen
            for (int OffsetY = filterOffset; OffsetY < height - filterOffset; OffsetY++)
            {
                for (int OffsetX = filterOffset; OffsetX < width - filterOffset; OffsetX++)
                {
                    //restablecer los valores rgb a 0
                    xr = xg = xb = 0;
                    rt = gt = bt = 0.0;

                    //posición del píxel del centro del núcleo
                    byteOffset = OffsetY * srcData.Stride + OffsetX * 4;

                    //cálculos del kernel
                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + filterX * 4 + filterY * srcData.Stride;
                            xb += (double)(pixelBuffer[calcOffset]) * kernel[filterY + filterOffset, filterX + filterOffset];
                            xg += (double)(pixelBuffer[calcOffset + 1]) * kernel[filterY + filterOffset, filterX + filterOffset];
                            xr += (double)(pixelBuffer[calcOffset + 2]) * kernel[filterY + filterOffset, filterX + filterOffset];

                        }
                    }

                    //valores rgb totales para este píxel
                    bt = Math.Sqrt(xb * xb);
                    gt = Math.Sqrt(xg * xg);
                    rt = Math.Sqrt(xr * xr);

                    //establecer límites, los bytes pueden contener valores desde 0 hasta 255;
                    if (bt > 255) bt = 255;
                    else if (bt < 0) bt = 0;
                    if (gt > 255) gt = 255;
                    else if (gt < 0) gt = 0;
                    if (rt > 255) rt = 255;
                    else if (rt < 0) rt = 0;

                    //establezca nuevos datos en la otra matriz de bytes para sus datos de imagen
                    resultBuffer[byteOffset] = (byte)(bt);
                    resultBuffer[byteOffset + 1] = (byte)(gt);
                    resultBuffer[byteOffset + 2] = (byte)(rt);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }


            ///////////////////////////////     RESULTADO      //////////////////////

            //Cree un nuevo mapa de bits que contendrá los datos procesados
            Bitmap resultImage = new Bitmap(width, height);

            //Bloquear bits en la memoria del sistema
            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            //Copiar de la matriz de bytes que contiene los datos procesados ​​al mapa de bits
            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);

            //Devolver imagen procesada
            resultImage.UnlockBits(resultData);
            return resultImage;
        }

        static public Bitmap Laplaciano(Bitmap sourceImage)
        {
            int width = sourceImage.Width;
            int height = sourceImage.Height;


            BitmapData srcData = sourceImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int bytes = srcData.Stride * srcData.Height;

            byte[] pixelBuffer = new byte[bytes];
            byte[] resultBuffer = new byte[bytes];

            IntPtr srcScan0 = srcData.Scan0;

            Marshal.Copy(srcScan0, pixelBuffer, 0, bytes);

            sourceImage.UnlockBits(srcData);

            ///////////////////////////////     KERNEL      //////////////////////
            double[,] kernel =  {
                { 0, 1, 0 },
                { 1, -4, 1 },
                { 0, 1, 0 }
            };

            double xr = 0.0;
            double xg = 0.0;
            double xb = 0.0;
            double rt = 0.0;
            double gt = 0.0;
            double bt = 0.0;


            int filterOffset = 1;
            int calcOffset = 0;
            int byteOffset = 0;


            for (int OffsetY = filterOffset; OffsetY < height - filterOffset; OffsetY++)
            {
                for (int OffsetX = filterOffset; OffsetX < width - filterOffset; OffsetX++)
                {

                    xr = xg = xb = 0;
                    rt = gt = bt = 0.0;

                    //posición del píxel del centro del núcleo
                    byteOffset = OffsetY * srcData.Stride + OffsetX * 4;

                    //cálculos del kernel
                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + filterX * 4 + filterY * srcData.Stride;
                            xb += (double)(pixelBuffer[calcOffset]) * kernel[filterY + filterOffset, filterX + filterOffset];
                            xg += (double)(pixelBuffer[calcOffset + 1]) * kernel[filterY + filterOffset, filterX + filterOffset];
                            xr += (double)(pixelBuffer[calcOffset + 2]) * kernel[filterY + filterOffset, filterX + filterOffset];

                        }
                    }


                    bt = Math.Sqrt(xb * xb);
                    gt = Math.Sqrt(xg * xg);
                    rt = Math.Sqrt(xr * xr);


                    if (bt > 255) bt = 255;
                    else if (bt < 0) bt = 0;
                    if (gt > 255) gt = 255;
                    else if (gt < 0) gt = 0;
                    if (rt > 255) rt = 255;
                    else if (rt < 0) rt = 0;

                    resultBuffer[byteOffset] = (byte)(bt);
                    resultBuffer[byteOffset + 1] = (byte)(gt);
                    resultBuffer[byteOffset + 2] = (byte)(rt);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }


            ///////////////////////////////     RESULTADO      //////////////////////
            Bitmap resultImage = new Bitmap(width, height);
            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultImage.UnlockBits(resultData);
            return resultImage;
        }

        static public Bitmap DireccionalNorte(Bitmap sourceImage)
        {
            int width = sourceImage.Width;
            int height = sourceImage.Height;

            BitmapData srcData = sourceImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            int bytes = srcData.Stride * srcData.Height;

            byte[] pixelBuffer = new byte[bytes];
            byte[] resultBuffer = new byte[bytes];

            IntPtr srcScan0 = srcData.Scan0;

            Marshal.Copy(srcScan0, pixelBuffer, 0, bytes);

            sourceImage.UnlockBits(srcData);


            ///////////////////////////////     KERNEL      //////////////////////
            double[,] kernel =  {
                { 1, 1, 1 },
                { 1, -2, 1 },
                { -1, -1, -1}
            };

            double xr = 0.0;
            double xg = 0.0;
            double xb = 0.0;
            double rt = 0.0;
            double gt = 0.0;
            double bt = 0.0;

            int filterOffset = 1;
            int calcOffset = 0;
            int byteOffset = 0;


            for (int OffsetY = filterOffset; OffsetY < height - filterOffset; OffsetY++)
            {
                for (int OffsetX = filterOffset; OffsetX < width - filterOffset; OffsetX++)
                {
                    xr = xg = xb = 0;
                    rt = gt = bt = 0.0;

                    byteOffset = OffsetY * srcData.Stride + OffsetX * 4;

                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + filterX * 4 + filterY * srcData.Stride;
                            xb += (double)(pixelBuffer[calcOffset]) * kernel[filterY + filterOffset, filterX + filterOffset];
                            xg += (double)(pixelBuffer[calcOffset + 1]) * kernel[filterY + filterOffset, filterX + filterOffset];
                            xr += (double)(pixelBuffer[calcOffset + 2]) * kernel[filterY + filterOffset, filterX + filterOffset];

                        }
                    }

                    bt = Math.Sqrt(xb * xb);
                    gt = Math.Sqrt(xg * xg);
                    rt = Math.Sqrt(xr * xr);

                    if (bt > 255) bt = 255;
                    else if (bt < 0) bt = 0;
                    if (gt > 255) gt = 255;
                    else if (gt < 0) gt = 0;
                    if (rt > 255) rt = 255;
                    else if (rt < 0) rt = 0;

                   
                    resultBuffer[byteOffset] = (byte)(bt);
                    resultBuffer[byteOffset + 1] = (byte)(gt);
                    resultBuffer[byteOffset + 2] = (byte)(rt);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }


            ///////////////////////////////     RESULTADO      //////////////////////
            Bitmap resultImage = new Bitmap(width, height);
            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultImage.UnlockBits(resultData);
            return resultImage;
        }

        static public Bitmap Sobel(Bitmap sourceImage, string tipo)
        {
            int width = sourceImage.Width;
            int height = sourceImage.Height;

            BitmapData srcData = sourceImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int bytes = srcData.Stride * srcData.Height;
            byte[] pixelBuffer = new byte[bytes];
            byte[] resultBuffer = new byte[bytes];
            IntPtr srcScan0 = srcData.Scan0;
            Marshal.Copy(srcScan0, pixelBuffer, 0, bytes);
           sourceImage.UnlockBits(srcData);

            ///////////////////////////////ESCALA DE GRISES//////////////////////
            float rgb = 0;
            for (int i = 0; i < pixelBuffer.Length; i += 4)
            {
                rgb = pixelBuffer[i] * .21f;        //R
                rgb += pixelBuffer[i + 1] * .71f;   //G
                rgb += pixelBuffer[i + 2] * .071f;  //B
                pixelBuffer[i] = (byte)rgb;
                pixelBuffer[i + 1] = pixelBuffer[i];
                pixelBuffer[i + 2] = pixelBuffer[i];
                pixelBuffer[i + 3] = 255;           //A
            }

            ///////////////////////////////     KERNEL      //////////////////////
            double xr = 0.0;
            double xg = 0.0;
            double xb = 0.0;
            double yr = 0.0;
            double yg = 0.0;
            double yb = 0.0;
            double rt = 0.0;
            double gt = 0.0;
            double bt = 0.0;

            int filterOffset = 1;
            int calcOffset = 0;
            int byteOffset = 0;

            double[,] xkernel =  {
                        { -1, 0, 1 },
                        { -2, 0, 2 },
                        { -1, 0, 1 }
                    };
            double[,] ykernel = {
                        {  1,  2,  1 },
                        {  0,  0,  0 },
                        { -1, -2, -1 }
                    };


            for (int OffsetY = filterOffset; OffsetY < height - filterOffset; OffsetY++)
            {
                for (int OffsetX = filterOffset; OffsetX < width - filterOffset; OffsetX++)
                {
                    xr = xg = xb = yr = yg = yb = 0;
                    rt = gt = bt = 0.0;

                    byteOffset = OffsetY * srcData.Stride + OffsetX * 4;

                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + filterX * 4 + filterY * srcData.Stride;
                            xb += (double)(pixelBuffer[calcOffset]) * xkernel[filterY + filterOffset, filterX + filterOffset];
                            xg += (double)(pixelBuffer[calcOffset + 1]) * xkernel[filterY + filterOffset, filterX + filterOffset];
                            xr += (double)(pixelBuffer[calcOffset + 2]) * xkernel[filterY + filterOffset, filterX + filterOffset];
                            yb += (double)(pixelBuffer[calcOffset]) * ykernel[filterY + filterOffset, filterX + filterOffset];
                            yg += (double)(pixelBuffer[calcOffset + 1]) * ykernel[filterY + filterOffset, filterX + filterOffset];
                            yr += (double)(pixelBuffer[calcOffset + 2]) * ykernel[filterY + filterOffset, filterX + filterOffset];
                        }
                    }

                    if (tipo == "x")
                    {
                        bt = Math.Sqrt(xb * xb);
                        gt = Math.Sqrt(xg * xg);
                        rt = Math.Sqrt(xr * xr);
                    }

                    if (tipo == "y")
                    {
                        bt = Math.Sqrt(yb * yb);
                        gt = Math.Sqrt(yg * yg);
                        rt = Math.Sqrt(yr * yr);
                    }

                    if (tipo == "xy")
                    {
                        bt = Math.Sqrt((xb * xb) + (yb * yb));
                        gt = Math.Sqrt((xg * xg) + (yg * yg));
                        rt = Math.Sqrt((xr * xr) + (yr * yr));
                    }

                    if (bt > 255) bt = 255;
                    else if (bt < 0) bt = 0;
                    if (gt > 255) gt = 255;
                    else if (gt < 0) gt = 0;
                    if (rt > 255) rt = 255;
                    else if (rt < 0) rt = 0;

                    resultBuffer[byteOffset] = (byte)(bt);
                    resultBuffer[byteOffset + 1] = (byte)(gt);
                    resultBuffer[byteOffset + 2] = (byte)(rt);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }


            ///////////////////////////////     RESULTADO      //////////////////////
            Bitmap resultImage = new Bitmap(width, height);
            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultImage.UnlockBits(resultData);
            return resultImage;
        }

        static public Bitmap FiltroRGB(Bitmap sourceImage)
        {
            float canalR = Globals.canalR;
            float canalG = Globals.canalG;
            float canalB = Globals.canalB;

            Image img = sourceImage;
            Bitmap bmpinverted = new Bitmap(img.Width, img.Height);

            ImageAttributes Ia = new ImageAttributes();
            ColorMatrix cmPicture = new ColorMatrix(new float[][]
            {
                        //Matrix RGBAW con los valores para el filtro
                        new float[]{ canalR, 0,0, 0, 0},    // red scaling factor 
                        new float[]{ 0, canalG, 0, 0, 0},   // green scaling factor
                        new float[]{ 0, 0, canalB, 0, 0},   // blue scaling factor
                        new float[]{0, 0, 0, 1, 0},             // alpha scaling factor
                        new float[]{0, 0, 0, 0, 1}              // three translations
            });
            Ia.SetColorMatrix(cmPicture);
            Graphics gr = Graphics.FromImage(bmpinverted);

            gr.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, Ia);
            gr.Dispose();


            return bmpinverted;
        }
        static public Bitmap AplicarFiltro(Bitmap sourceImage, string filtro)
        {
            Bitmap resultImage = null ;

            switch (filtro)
            {
                case "Gaussiano":
                    resultImage = Filters.Gaussiano(sourceImage);
                    break;

                case "Sal y pimienta":
                    resultImage = Filters.SalYPimienta(sourceImage);
                    break;

                case "Enfocar":
                    resultImage = Filters.Enfocar(sourceImage);
                    break;

                case "Laplaciano":
                    resultImage = Filters.Laplaciano(sourceImage);
                    break;

                case "Sobel (x, y)":
                    resultImage = Filters.Sobel(sourceImage, "xy");
                    break;
                case "Sobel (derivada en x)":
                    resultImage = Filters.Sobel(sourceImage, "x");
                    break;
                case "Sobel (derivada en y)":
                    resultImage = Filters.Sobel(sourceImage, "y");
                    break;

                case "Direccional Norte":
                    resultImage = Filters.DireccionalNorte(sourceImage);
                    break;

                case "Canales RGB":
                    resultImage = Filters.FiltroRGB(sourceImage);
                    break;

                default:
                    break;
            }

            return resultImage;
        }

        


    }
}
