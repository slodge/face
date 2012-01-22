using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SmilieHappy.Helpers
{
    public static class WriteableBitmapExtensions
    {
        public static WriteableBitmap MakeSmallerCopy(this WriteableBitmap original, int maxDimension)
        {
            var ratio = 0.0;
            if (original.PixelHeight > original.PixelWidth)
            {
                ratio = ((double)original.PixelHeight) / maxDimension;
            }
            else
            {
                ratio = ((double)original.PixelWidth) / maxDimension;
            }
            if (ratio > 1.0)
                ratio = 1.0;
            var newWidth = (int)Math.Floor(((double)original.PixelWidth) / ratio);
            var newHeight = (int)Math.Floor(((double)original.PixelHeight) / ratio);
            var toReturn = new WriteableBitmap(newWidth, newHeight);
            toReturn.Blit(toReturn.BoundingRect(), original, original.BoundingRect());
            return toReturn;
        }

        public static Rect BoundingRect(this WriteableBitmap bmp)
        {
            var rect = new Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight);
            return rect;
        }
    }
}