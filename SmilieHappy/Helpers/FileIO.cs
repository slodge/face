using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace SmilieHappy.Helpers
{
    public static class FileIO
    {
        public static void SaveToFile(this BitmapImage bitmapImage, string tempFileName)
        {
            var wb = new WriteableBitmap(bitmapImage);
            SaveToFile(wb, tempFileName, (stream) => { });
        }

        public static void SaveToFile(this WriteableBitmap wb, string tempFileName, Action<Stream> extra)
        {
            wb.SaveToFile(tempFileName, AppConstants.DefaultJpegQuality, extra);
        }

        public static void SaveToFile(this WriteableBitmap wb, string tempFileName, int jpegQuality, Action<Stream> extra)
        {
            using (var iso = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (iso.FileExists(tempFileName))
                {
                    iso.DeleteFile(tempFileName);
                }

                using (var fileStream = iso.CreateFile(tempFileName))
                {
                    // Encode WriteableBitmap object to a JPEG stream.
                    wb.SaveJpeg(fileStream, wb.PixelWidth, wb.PixelHeight, 0, jpegQuality);
                    extra(fileStream);
                }
            }
        }

        public static BitmapImage LoadFromFile(string fileName)
        {
            var toReturn = new BitmapImage();
            LoadFromFile(fileName, toReturn.SetSource);
            return toReturn;
        }

        public static void LoadFromFile(string fileName, Action<Stream> loadAction)
        {
            using (var iso = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var fileStream = iso.OpenFile(fileName, FileMode.Open))
                {
                    loadAction(fileStream);
                }
            }
        }
    }
}
