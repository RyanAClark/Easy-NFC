using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Easynfc
{
    public partial class Report_Bug : PhoneApplicationPage
    {
        public Report_Bug()
        {
            InitializeComponent();
         
        }

        private void Go_Click(object sender, RoutedEventArgs e)
        {
            if ((URL.Text.Substring(0, 7) == "http://") && (URL.Text.Length > 6))
            {
                string site = URL.Text;
                MiniBrowser.Navigate(new Uri(site, UriKind.Absolute));
            }
            else
            {
                string site = "http://" + URL.Text;
                MiniBrowser.Navigate(new Uri(site, UriKind.Absolute));
            }
        }
    }
}


