
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Easynfc.Resources;
using NdefLibrary.Ndef;
using Windows.Networking.Proximity;
using Windows.Phone.Speech.Recognition;
using Windows.Phone.Speech.Synthesis;
using Microsoft.Phone.Tasks;
using System.Device.Location;
 



namespace Easynfc
{
    public partial class MainPage : PhoneApplicationPage
    {
       
        private long _subscriptionIdNdef;
        private long _publishingMessageId;

        private ProximityDevice _device;
        private SpeechRecognizer _recognizer;
        private SpeechSynthesizer _synthesizer;
        bool check = false;
        int Sup = 0;
        string recordresult;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            // Update enabled / disabled state of buttons in the User Interface
            UpdateUiForNfcStatus();
            // Initialize app menu
            BuildLocalizedApplicationBar();
            _device = ProximityDevice.GetDefault();
            _recognizer = new SpeechRecognizer();
            _synthesizer = new SpeechSynthesizer();




          




        }

     /*private void BtnInitNfc_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Initialize NFC
            _device = ProximityDevice.GetDefault();
            // Update status text for UI
            SetStatusOutput(_device != null ? AppResources.StatusInitialized : AppResources.StatusInitFailed);
            // Update enabled / disabled state of buttons in the User Interface
            UpdateUiForNfcStatus();
        }*/

        private void BtnSubscribeNdef_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Only subscribe for messages if no NDEF subscription is already active
            if (_subscriptionIdNdef != 0) return;
            // Ask the proximity device to inform us about any kind of NDEF message received from
            // another device or tag.
            // Store the subscription ID so that we can cancel it later.
            _subscriptionIdNdef = _device.SubscribeForMessage("NDEF", MessageReceivedHandler);
            // Update status text for UI
            SetStatusOutput(string.Format(AppResources.StatusSubscribed, _subscriptionIdNdef));
            // Update enabled / disabled state of buttons in the User Interface
            UpdateUiForNfcStatus();
        }

        private void MessageReceivedHandler(ProximityDevice sender, ProximityMessage message)
        {
            // Get the raw NDEF message data as byte array
            var rawMsg = message.Data.ToArray();
            // Let the NDEF library parse the NDEF message out of the raw byte array
            var ndefMessage = NdefMessage.FromByteArray(rawMsg);

            // Analysis result
            var tagContents = new StringBuilder();
           
            // Loop over all records contained in the NDEF message
            foreach (NdefRecord record in ndefMessage)
            {
                // --------------------------------------------------------------------------
                // Print generic information about the record
                if (record.Id != null && record.Id.Length > 0)
                {
                    // Record ID (if present)
                    tagContents.AppendFormat("Id: {0}\n", Encoding.UTF8.GetString(record.Id, 0, record.Id.Length));
                }
                // Record type name, as human readable string
                tagContents.AppendFormat("Type name: {0}\n", ConvertTypeNameFormatToString(record.TypeNameFormat));
                // Record type
                if (record.Type != null && record.Type.Length > 0)
                {
                    tagContents.AppendFormat("Record type: {0}\n",
                                             Encoding.UTF8.GetString(record.Type, 0, record.Type.Length));
                }

                // --------------------------------------------------------------------------
                // Check the type of each record
                // Using 'true' as parameter for CheckSpecializedType() also checks for sub-types of records,
                // e.g., it will return the SMS record type if a URI record starts with "sms:"
                // If using 'false', a URI record will always be returned as Uri record and its contents won't be further analyzed
                // Currently recognized sub-types are: SMS, Mailto, Tel, Nokia Accessories, NearSpeak, WpSettings
                var specializedType = record.CheckSpecializedType(true);

                if (specializedType == typeof(NdefMailtoRecord))
                {
                    // --------------------------------------------------------------------------
                    // Convert and extract Mailto record info
                    var mailtoRecord = new NdefMailtoRecord(record);
                    tagContents.Append("-> Mailto record\n");
                    tagContents.AppendFormat("Address: {0}\n", mailtoRecord.Address);
                    tagContents.AppendFormat("Subject: {0}\n", mailtoRecord.Subject);
                    tagContents.AppendFormat("Body: {0}\n", mailtoRecord.Body);
                }
                else if (specializedType == typeof(NdefUriRecord))
                {
                    // --------------------------------------------------------------------------
                    // Convert and extract URI record info
                    var uriRecord = new NdefUriRecord(record);
                    tagContents.Append("-> URI record\n");
                    tagContents.AppendFormat("URI: {0}\n", uriRecord.Uri);
                }
                else if (specializedType == typeof(NdefSpRecord))
                {
                    // --------------------------------------------------------------------------
                    // Convert and extract Smart Poster info
                    var spRecord = new NdefSpRecord(record);
                    tagContents.Append("-> Smart Poster record\n");
                    tagContents.AppendFormat("URI: {0}", spRecord.Uri);
                    tagContents.AppendFormat("Titles: {0}", spRecord.TitleCount());
                    if (spRecord.TitleCount() > 1)
                        tagContents.AppendFormat("1. Title: {0}", spRecord.Titles[0].Text);
                    tagContents.AppendFormat("Action set: {0}", spRecord.ActionInUse());
                    // You can also check the action (if in use by the record), 
                    // mime type and size of the linked content.
                }
                else if (specializedType == typeof (NdefLaunchAppRecord))
                {
                    
                    // --------------------------------------------------------------------------
                    // Convert and extract LaunchApp record info
                    var launchAppRecord = new NdefLaunchAppRecord(record);
                    tagContents.Append("-> LaunchApp record" + Environment.NewLine);
                   
                    if (!string.IsNullOrEmpty(launchAppRecord.Arguments))
                     
                        tagContents.AppendFormat("Arguments: {0}\n", launchAppRecord.Arguments);
                    if (launchAppRecord.PlatformIds != null)
                    {
                        foreach (var platformIdTuple in launchAppRecord.PlatformIds)
                        {
                            if (platformIdTuple.Key != null)
                                tagContents.AppendFormat("Platform: {0}\n", platformIdTuple.Key);
                            if (platformIdTuple.Value != null)
                                tagContents.AppendFormat("App ID: {0}\n", platformIdTuple.Value);
                        }
                    }
                }
                else
                {
                    // Other type, not handled by this demo
                    tagContents.Append("NDEF record not parsed by this demo app" + Environment.NewLine);
                }
            }
            // Update status text for UI
            SetStatusOutput(string.Format(AppResources.StatusTagParsed, tagContents));
        }

        private void BtnWriteLaunchApp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Sup = 0;
            // Create a new LaunchApp record to launch this app
            // The app will print the arguments when it is launched (see MainPage.OnNavigatedTo() method)
            var record = new NdefLaunchAppRecord { Arguments = write_to_tag_box.Text};
            // WindowsPhone is the pre-defined platform ID for WP8.
            // You can get the application ID from the WMAppManifest.xml file
            record.AddPlatformAppId("WindowsPhone", "{544ec154-b521-4d73-9405-963830adb213}");
            // Publish the record using the proximity device
            recordresult = record.Arguments;
            PublishRecord(record, true);
            Sup = 1;
           
        }


        private void PublishRecord(NdefRecord record, bool writeToTag)
        {
            if (_device == null) return;
            // Make sure we're not already publishing another message
            StopPublishingMessage(false);
            // Wrap the NDEF record into an NDEF message
            var message = new NdefMessage { record };
            // Convert the NDEF message to a byte array
            var msgArray = message.ToByteArray();
            // Publish the NDEF message to a tag or to another device, depending on the writeToTag parameter
            // Save the publication ID so that we can cancel publication later
            _publishingMessageId = _device.PublishBinaryMessage((writeToTag ? "NDEF:WriteTag" : "NDEF"), msgArray.AsBuffer(), MessageWrittenHandler);
            // Update status text for UI
            SetStatusOutput(string.Format((writeToTag ? AppResources.StatusWriteToTag : AppResources.StatusWriteToDevice), msgArray.Length, _publishingMessageId));
            // Update enabled / disabled state of buttons in the User Interface
            SetStatusOutput(string.Format("You said \"{0}\"\nPlease touch a tag to write the message.", recordresult));
           



            recordresult = "";
            UpdateUiForNfcStatus();
        }

        private void MessageWrittenHandler(ProximityDevice sender, long messageid)
        {
            // Stop publishing the message
            StopPublishingMessage(false);
            // Update status text for UI
            SetStatusOutput(AppResources.StatusMessageWritten);
            //Dispatcher.BeginInvoke(() => MessageBox.Show("NFC Message written"));

        }

        private void SetStatusOutput(string newStatus)
        {
            

            // Update the status output UI element in the UI thread
            // (some of the callbacks are in a different thread that wouldn't be allowed
            // to modify the UI thread)
            Dispatcher.BeginInvoke(() => { if (StatusOutput != null) StatusOutput.Text = newStatus; });
           
        }

        private string ConvertTypeNameFormatToString(NdefRecord.TypeNameFormatType tnf)
        {
            // Each record contains a type name format, which defines which format
            // the type name is actually in.
            // This method converts the constant to a human-readable string.
            string tnfString;
            switch (tnf)
            {
                case NdefRecord.TypeNameFormatType.Empty:
                    tnfString = "Empty NDEF record (does not contain a payload)";
                    break;
                case NdefRecord.TypeNameFormatType.NfcRtd:
                    tnfString = "NFC RTD Specification";
                    break;
                case NdefRecord.TypeNameFormatType.Mime:
                    tnfString = "RFC 2046 (Mime)";
                    break;
                case NdefRecord.TypeNameFormatType.Uri:
                    tnfString = "RFC 3986 (Url)";
                    break;
                case NdefRecord.TypeNameFormatType.ExternalRtd:
                    tnfString = "External type name";
                    break;
                case NdefRecord.TypeNameFormatType.Unknown:
                    tnfString = "Unknown record type; should be treated similar to content with MIME type 'application/octet-stream' without further context";
                    break;
                case NdefRecord.TypeNameFormatType.Unchanged:
                    tnfString = "Unchanged (partial record)";
                    break;
                case NdefRecord.TypeNameFormatType.Reserved:
                    tnfString = "Reserved";
                    break;
                default:
                    tnfString = "Unknown";
                    break;
            }
            return tnfString;
        }


        private void BtnStopSubscription_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Stop NDEF subscription and print status update to the UI
            StopSubscription(true);
        }

        private void StopSubscription(bool writeToStatusOutput)
        {
            if (_subscriptionIdNdef != 0 && _device != null)
            {
                // Ask the proximity device to stop subscribing for NDEF messages
                _device.StopSubscribingForMessage(_subscriptionIdNdef);
                _subscriptionIdNdef = 0;
                // Update enabled / disabled state of buttons in the User Interface
                UpdateUiForNfcStatus();
                // Update status text for UI - only if activated
                if (writeToStatusOutput) SetStatusOutput(AppResources.StatusSubscriptionStopped);
            }
        }

        private void BtnStopPublication_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StopPublishingMessage(true);
        }

        private void StopPublishingMessage(bool writeToStatusOutput)
        {
            if (_publishingMessageId != 0 && _device != null)
            {
                // Stop publishing the message
                _device.StopPublishingMessage(_publishingMessageId);
                _publishingMessageId = 0;
                // Update enabled / disabled state of buttons in the User Interface
                UpdateUiForNfcStatus();
                // Update status text for UI - only if activated
                if (writeToStatusOutput) SetStatusOutput(AppResources.StatusPublicationStopped);
            }
        }

        private void UpdateUiForNfcStatus()
        {
            Dispatcher.BeginInvoke(() =>
                                       {
                                         //  BtnInitNfc.IsEnabled = (_device == null);

                                           // Subscription buttons
                                           BtnSubscribeNdef.IsEnabled = (_device != null && _subscriptionIdNdef == 0);
                                           BtnStopSubscription.IsEnabled = (_device != null && _subscriptionIdNdef != 0);

                                           // Publishing buttons
                                           BtnWriteLaunchApp.IsEnabled = (_device != null && _publishingMessageId == 0);
                                           BtnStopPublication.IsEnabled = (_device != null && _publishingMessageId != 0);
                                       });
        }

        protected override async void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Check if app was launched via LaunchApp tag
            if (NavigationContext.QueryString.ContainsKey("ms_nfp_launchargs"))
            {
                if (NavigationContext.QueryString["ms_nfp_launchargs"] == "map1234")
                {
                    BingMapsTask map = new BingMapsTask();
                    map.Center = new GeoCoordinate();
                    map.Show();
                    NavigationContext.QueryString["ms_nfp_launchargs"] = "EasyNfc";
                    Sup = 1;
                }
               





                    // Update status text for UI
                    // Print arguments retrieved from LaunchApp tag
                    SetStatusOutput(string.Format(AppResources.StatusLaunchedFromTag, NavigationContext.QueryString["ms_nfp_launchargs"]));
                    if (Sup == 0)
                    {

                        await _synthesizer.SpeakTextAsync(NavigationContext.QueryString["ms_nfp_launchargs"]);
                        Sup = 1;
                    }


                
            }
        }


        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar { Mode = ApplicationBarMode.Minimized };

            // Create a new menu item with the localized string from AppResources.
            var appBarMenuItemAbout = new ApplicationBarMenuItem(AppResources.MenuAbout);
            ApplicationBar.MenuItems.Add(appBarMenuItemAbout);
            appBarMenuItemAbout.Click += CmdAbout;
        }

        private void CmdAbout(object sender, EventArgs e)
        {
            // Navigate to the about page
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void Button_Click2(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Report_Bug.xaml", UriKind.Relative));
        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/CreateCustomFunction.xaml", UriKind.Relative));
        }

        private async void ListenBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Sup = 0;
            if(check==false){
                check = true;
                SetStatusOutput(string.Format("RECORDING"));
            // Start speech recognition and wait for the async process to finish
            var recoResult = await _recognizer.RecognizeAsync();
            // Inform the user about the result and the next steps
            SetStatusOutput(string.Format("You said \"{0}\"\nPlease touch a tag to write the message.", recoResult.Text));
            // Create a LaunchApp record, specifying our recognized text as arguments
         
           var record = new NdefLaunchAppRecord { Arguments = recoResult.Text };
            // Add the app ID of your app!
          record.AddPlatformAppId("WindowsPhone", "{544ec154-b521-4d73-9405-963830adb213}");
            // Wrap the record into a message, which can be written to a tag       

            recordresult = recoResult.Text;
            PublishRecord(record, true);
            check = false;
            Sup = 1;
            }
           
        }

        private void map_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            //Sup = 1;
            var record = new NdefLaunchAppRecord { Arguments = "map1234" };
            // WindowsPhone is the pre-defined platform ID for WP8.
            // You can get the application ID from the WMAppManifest.xml file
            record.AddPlatformAppId("WindowsPhone", "{544ec154-b521-4d73-9405-963830adb213}");
            // Publish the record using the proximity device
            recordresult = record.Arguments;
            PublishRecord(record, true);




        }

        private void Tutorial_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/NewTutorial.xaml", UriKind.Relative));
        }
       
       
    }
}