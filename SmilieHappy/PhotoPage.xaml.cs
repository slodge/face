using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Coding4Fun.Phone.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework.Media;
using SmilieHappy.Helpers;

namespace SmilieHappy
{
    public partial class PhotoPage : PhoneApplicationPage
    {
        private string _fileToSaveName = "base";

        public PhotoPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            HackyFindTitleInUri(e.Uri);
            _fileToSaveName = "Smilie.jpg";
            SelectedImage.Source = FileIO.LoadFromFile(AppConstants.IsoFileName);
        }

        private void HackyFindTitleInUri(Uri uri)
        {
            var raw = HackyFindTitleInUriInner(uri);
            var split = raw.Split(',');
            var happy = split[0];
            var sad = split[1];
            PageTitleHappy.Text = happy;
            PageTitleSad.Text = sad;
        }

        private static string HackyFindTitleInUriInner(Uri uri)
        {
            var query = uri.OriginalString;
            if (string.IsNullOrEmpty(query))
                return "0,0";

            var amountString = "";
            for (var i = query.Length - 1; i >= 0; i--)
            {
                var current = query[i];
                if (current == '=')
                    break;

                amountString = current + amountString;
            }
            return amountString;
        }

        private void AppBarSaveButton_Click(object sender, EventArgs e)
        {
            var library = new MediaLibrary();
            Picture picture;
            FileIO.LoadFromFile(AppConstants.IsoFileName, (stream) => picture = library.SavePicture(_fileToSaveName, stream));

            var toast = new ToastPrompt()
            {
                Message = "Saved",
                MillisecondsUntilHidden = 1500,
                Title = ":) or :("
            };
            toast.Show();

            /*
             * 
             * would love to open the picture here!
            var launcher = new MediaPlayerLauncher()
            {
                Media = picture.
            }
            ;
             */
        }
    }
}