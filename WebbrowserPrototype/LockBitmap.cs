using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using SHDocVw;

namespace WebbrowserPrototype
{
    public class LockBitmap
    {
        private BitmapData bitmapData;
        private IntPtr Iptr = IntPtr.Zero;
        private readonly Bitmap source;

        public LockBitmap(Bitmap source)
        {
            this.source = source;
        }

        public byte[] Pixels { get; set; }

        /// <summary>
        ///     Lock bitmap data
        /// </summary>
        public void LockBits(int width, int height)
        {
            try
            {
                // get total locked pixels count
                var PixelCount = width*height;

                // Create rectangle to lock
                var rect = new Rectangle(0, 0, width, height);

                // get source bitmap pixel format size
                var depth = Image.GetPixelFormatSize(source.PixelFormat);

                // Check if bpp (Bits Per Pixel) is 8, 24, or 32
                if (depth != 8 && depth != 24 && depth != 32)
                {
                    throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
                }

                // Lock bitmap and return bitmap data
                bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite,
                    source.PixelFormat);

                // create byte array to copy pixel values
                var step = depth / 8;
                Pixels = new byte[PixelCount*step];
                Iptr = bitmapData.Scan0;

                // Copy data from pointer to array
                Marshal.Copy(Iptr, Pixels, 0, Pixels.Length);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///     Unlock bitmap data
        /// </summary>
        public void UnlockBits()
        {
            try
            {
                // Copy data from byte array to pointer
                Marshal.Copy(Pixels, 0, Iptr, Pixels.Length);

                // Unlock bitmap data
                source.UnlockBits(bitmapData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}