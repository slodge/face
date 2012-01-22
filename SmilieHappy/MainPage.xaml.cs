using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using SmilieHappy.Helpers;

namespace SmilieHappy
{
    public partial class MainPage : PhoneApplicationPage
    {
        private WriteableBitmap _smilieWriteableBitmap;
        private WriteableBitmap _unsmilieWriteableBitmap;
        private WriteableBitmap _sampleBitmap;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            LoadSmilies();
        }

        private void LoadSmilies()
        {
            _smilieWriteableBitmap = LoadWriteable("Images/Smilie1.png");
            _unsmilieWriteableBitmap = LoadWriteable("Images/UnSmilie1.png");
            _sampleBitmap = LoadWriteable("Images/shezammsample.jpg");
            SelectedImage.Source = _smilieWriteableBitmap;
        }

        private static WriteableBitmap LoadWriteable(string resourcePath)
        {
            var stream = App.GetResourceStream(new Uri(resourcePath, UriKind.Relative));
            var bitmap = new BitmapImage();
            bitmap.SetSource(stream.Stream);
            var writeable = new WriteableBitmap(bitmap);
            return writeable;
        }

        private void DisplayResult(WriteableBitmap writeableBitmap, FaceRestAPI.FaceAPI result)
        {
            DisplaySmilieResult(writeableBitmap, result);
        }

        private void DisplaySmilieResult(WriteableBitmap writeableBitmap, FaceRestAPI.FaceAPI result)
        {
            int countHappy = 0;
            int countSad = 0;
            foreach (var tag in result.photos[0].tags)
            {
                // Uses the segment's center and half width, height
                var c = tag.center;
                Debug.WriteLine("Center ({0} ,{1}), size ({2}, {3})", c.x, c.y, tag.width,
                                tag.height);
                var destRect = new Rect(writeableBitmap.PixelWidth * 0.01 * (tag.center.x - tag.width / 2),
                                        writeableBitmap.PixelHeight * 0.01 * (tag.center.y - tag.height / 2),
                                        writeableBitmap.PixelWidth * 0.01 * tag.width,
                                        writeableBitmap.PixelHeight * 0.01 * tag.height);
                var whichFace = ChooseFace(tag.attributes);
                if (whichFace == _smilieWriteableBitmap)
                    countHappy++;
                else
                    countSad++;
                var sourceRect = new Rect(0, 0, whichFace.PixelWidth, whichFace.PixelHeight);
                writeableBitmap.Blit(destRect, whichFace, sourceRect);
            }

            writeableBitmap.Invalidate();
            SelectedImage.Source = writeableBitmap;

            writeableBitmap.SaveToFile(AppConstants.IsoFileName, (stream) => { /* do nothing */ });
            ShowDisplay(DisplayState.Finished);


            NavigateToPhotoPage(countHappy, countSad);
        }

        private WriteableBitmap ChooseFace(Dictionary<string, FaceRestAPI.Confidence> confidences)
        {
            if (confidences == null)
                return _unsmilieWriteableBitmap;

            FaceRestAPI.Confidence smilieConfidence;
            if (!confidences.TryGetValue("smiling", out smilieConfidence))
                return _unsmilieWriteableBitmap;

            if (smilieConfidence.value == "true")
                return _smilieWriteableBitmap;
            else
                return _unsmilieWriteableBitmap;
        }

        private void ApplicationBarIconButton_Camera_Click(object sender, EventArgs e)
        {
            var task = new CameraCaptureTask();
            DoPhotoTask(task);
        }

        private void ApplicationBarIconButton_Picture_Click(object sender, EventArgs e)
        {
            var task = new PhotoChooserTask()
            {
                ShowCamera = true
            };
            DoPhotoTask(task);
        }

        private void ApplicationBarIconButton_About_Click(object sender, EventArgs e)
        {
            try
            {
                NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
            }
            catch (Exception)
            {
                // prevent against double taps
            }
        }

        private void DoPhotoTask(ChooserBase<PhotoResult> task)
        {
            try
            {
                task.Completed += (t, args) =>
                {
                    if (args.ChosenPhoto != null)
                    {
                        ShowDisplay(DisplayState.Processing);
                        var bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(args.ChosenPhoto);
                        var writeableBitmap = new WriteableBitmap(bitmapImage);
                        ProcessImage(writeableBitmap);
                    }
                    else
                    {
                        // hope the user doesn't mind being told nothing
                    }
                };
                task.Show();
            }
            catch (Exception exception)
            {
                // hope the user doesn't mind being told nothing
            }
        }


        private void ProcessImage(WriteableBitmap writeable)
        {
            var smaller = writeable.MakeSmallerCopy(AppConstants.MaxDimensionForUpload);
            smaller.SaveToFile(AppConstants.UploadFileName, AppConstants.JpegQualityForUpload, (stream) => { });
            
            var api = new FaceRestAPI(AppConstants.FaceApikey, AppConstants.FaceSecretkey, "", false, "json", "", "");
            api.faces_detect(null, 
                            AppConstants.UploadFileName, 
                            null, 
                            string.Empty,
                             (result) => Dispatcher.BeginInvoke(() => DisplayResult(writeable, result)),
                             (error) => Dispatcher.BeginInvoke(() =>
                                                                   {
                                                                       MessageBox.Show("Sorry - we had a problem: " + error);
                                                                       Debug.WriteLine("Bummer " + error);
                                                                       ShowDisplay(DisplayState.Intro);
                                                                   }));
        }

        private void NavigateToPhotoPage(int countHappy, int countSad)
        {
            Dispatcher.BeginInvoke(() =>
                                       {
                                           try
                                           {
                                               var title = string.Format("{0},{1}", countHappy, countSad);
                                               NavigationService.Navigate(new Uri("/PhotoPage.xaml?title=" + title, UriKind.Relative));
                                           }
                                           catch (Exception)
                                           {
                                               // prevent against double navigations
                                           }
                                       });
        }


        private enum DisplayState
        {
            Intro,
            Processing,
            Finished
        }

        private void ShowDisplay(DisplayState state)
        {
            switch (state)
            {
                case DisplayState.Intro:
                    IntroPanel.Visibility = Visibility.Visible;
                    ProcessingPanel.Visibility = Visibility.Collapsed;
                    FinishedPanel.Visibility = Visibility.Collapsed;                    
                    EnableApplicationBar(true);
                    break;
                case DisplayState.Processing:
                    ProcessingPanel.Visibility = Visibility.Visible;
                    IntroPanel.Visibility = Visibility.Collapsed;
                    FinishedPanel.Visibility = Visibility.Collapsed;                    
                    EnableApplicationBar(false);
                    break;
                case DisplayState.Finished:
                    ProcessingPanel.Visibility = Visibility.Collapsed;
                    IntroPanel.Visibility = Visibility.Collapsed;
                    FinishedPanel.Visibility = Visibility.Visible;                    
                    EnableApplicationBar(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("state");
            }
        }

        private void EnableApplicationBar(bool isEnabled)
        {
            foreach (ApplicationBarIconButton button in ApplicationBar.Buttons)
            {
                button.IsEnabled = isEnabled;
            }
        }
    }
}