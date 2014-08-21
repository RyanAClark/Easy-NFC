
using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Easynfc.Resources;

namespace Easynfc
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void BtnNdefLibrary_Click(object sender, RoutedEventArgs e)
        {
            LaunchUri(AppResources.AboutNdefLibraryUri);
        }

        private void BtnNfcInteractor_Click(object sender, RoutedEventArgs e)
        {
            LaunchUri(AppResources.AboutNfcInteractorUri);
        }

        private void BtnTwitter_Click(object sender, RoutedEventArgs e)
        {
            LaunchUri(AppResources.AboutTwitterUri);
        }

        private void LaunchUri(string uri)
        {
            var task = new WebBrowserTask { Uri = new Uri(uri, UriKind.Absolute) };
            task.Show();
        }
    }
}