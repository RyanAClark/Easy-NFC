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
    public partial class NewTutorial : PhoneApplicationPage
    {
        public NewTutorial()
        {
            InitializeComponent();
        }
        private void tutorial_2_button_Click(object sender, RoutedEventArgs e)
        {
            string text = "Work In Progress.";
            tutorial_text_box.Text = text;
        }

        private void tutorial_1_button_Click(object sender, RoutedEventArgs e)
        {
            string text = "To write to a tag first select a function to write to the tag, then follow the status text instructions in real time.";
            tutorial_text_box.Text = text;
        }

        private void tutorial_3_button_Click(object sender, RoutedEventArgs e)
        {
            string text = "Work in progress.";
            tutorial_text_box.Text = text;
        }

        private void tutorial_4_button_Click(object sender, RoutedEventArgs e)
        {
            string text = "Select the report bug button in the main menu and follow the on-screen instructions.";
            tutorial_text_box.Text = text;
        }
    }
}