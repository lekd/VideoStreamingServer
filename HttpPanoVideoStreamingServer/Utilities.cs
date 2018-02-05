using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HttpPanoVideoStreamingServer
{
    public class Utilities
    {
        public static BitmapImage ToBitmapImage(Bitmap bitmap, ImageFormat imgFormat)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, imgFormat);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                //bitmapImage.Freeze();
                return bitmapImage;
            }
        }
        public static Bitmap DownScaleBitmap(Bitmap origin, float downScaleFactor)
        {
            Bitmap resized = new Bitmap(origin, new Size((int)(origin.Width * 1.0f / downScaleFactor),
                                                           (int)(origin.Height * 1.0f / downScaleFactor)));
            return resized;
        }
        public static Bitmap CropBitmap(Bitmap origin, float percentL, float percentT, float percentW, float percentH)
        {
            
            Bitmap crop = new Bitmap((int)(percentW * origin.Width), (int)(percentH * origin.Height));
            Graphics g = Graphics.FromImage(crop);
            if (percentL >= 0 && percentL + percentW <= 1)
            {
                Rectangle cropRect = new Rectangle();
                cropRect.X = (int)(percentL * origin.Width);
                cropRect.Y = (int)(percentT * origin.Height);
                cropRect.Width = (int)(percentW * origin.Width);
                cropRect.Height = (int)(percentH * origin.Height);
                g.DrawImage(origin, new Rectangle(0, 0, crop.Width, crop.Height), cropRect, GraphicsUnit.Pixel);
            }
            else
            {
                Rectangle cropLeftHalf = new Rectangle();
                Rectangle cropRightHalf = new Rectangle();
                if (percentL < 0)
                {
                    cropRightHalf.X = 0;
                    cropRightHalf.Y = (int)(percentT * origin.Height);
                    cropRightHalf.Width = (int)((percentW + percentL) * origin.Width);
                    cropRightHalf.Height = (int)(percentH * origin.Height);

                    cropLeftHalf.X = (int)((1 + percentL) * origin.Width);
                    cropLeftHalf.Y = (int)(percentT * origin.Height);
                    cropLeftHalf.Width = (int)(-percentL * origin.Width);
                    cropLeftHalf.Height = (int)(percentH * origin.Height);
                }
                else
                {
                    cropLeftHalf.X = (int)(percentL * origin.Width);
                    cropLeftHalf.Y = (int)(percentT * origin.Height);
                    cropLeftHalf.Width = (int)((1 - percentL) * origin.Width);
                    cropLeftHalf.Height = (int)(percentH * origin.Height);

                    cropRightHalf.X = 0;
                    cropRightHalf.Y = (int)(percentT * origin.Height);
                    cropRightHalf.Width = (int)((percentL + percentW - 1) * origin.Width);
                    cropRightHalf.Height = (int)(percentH * origin.Height);
                }
                g.DrawImage(origin, new Rectangle(0, 0, cropLeftHalf.Width, cropLeftHalf.Height), cropLeftHalf, GraphicsUnit.Pixel);
                g.DrawImage(origin, new Rectangle(cropLeftHalf.Width, 0, cropRightHalf.Width, cropRightHalf.Height), cropRightHalf, GraphicsUnit.Pixel);
            }
            return crop;
        }
        public static byte[] BitmapToBytes(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
        public static int Bytes2Int(byte[] bytes, bool fromBigEndian)
        {
            if (BitConverter.IsLittleEndian & fromBigEndian)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToInt32(bytes, 0);
        }
        public static float Bytes2Float(byte[] bytes, bool fromBigEndian)
        {
            if (BitConverter.IsLittleEndian & fromBigEndian)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToSingle(bytes, 0);
        }
        public static byte[] Int2Bytes(int num, bool toBigEndian = true)
        {
            byte[] bytes = BitConverter.GetBytes(num);
            if (BitConverter.IsLittleEndian && toBigEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }
        public static byte[] Float2Bytes(float num, bool toBigEndian = true)
        {
            byte[] bytes = BitConverter.GetBytes(num);
            if (BitConverter.IsLittleEndian && toBigEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }
        public static int findNumberInByteArray(int searchedNumber, int startingOffset, byte[] data, int dataLength, bool isDataFromBigEndian)
        {
            int foundIndex = -1;
            byte[] buffer = new byte[4];
            for (int i = startingOffset; i < dataLength - 4; i++)
            {
                Array.Copy(data, i, buffer, 0, buffer.Length);
                int num = Utilities.Bytes2Int(buffer, isDataFromBigEndian);
                if (num == searchedNumber)
                {
                    foundIndex = i;
                    break;
                }
            }
            return foundIndex;
        }
        public static byte[] shiftArray(byte[] origin, int length, int shiftOffset)
        {
            Array.Copy(origin, shiftOffset, origin, 0, length - shiftOffset);
            return origin;
        }
    }
}
