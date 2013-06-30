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
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using RunToScore.Resources;

namespace RunToScore
{
    public partial class Share : PhoneApplicationPage
    {
        public Share()
        {
            InitializeComponent();
        }

        private void ShareByMessage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SmsComposeTask task = new SmsComposeTask();
            task.Body = AppResources.ShareMailSubject;
            task.Show();
        }

        private void ShareByMail_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //Create a new task
            EmailComposeTask task = new EmailComposeTask();
            //Add the current item’s EMail address
            //task.To = "runtoscore@gmail.com";
            //Just a little text for the message
            task.Subject = AppResources.ShareMailSubject;
            task.Body = AppResources.ShareMailSubject;
            task.Body += "Voici mes stats";
            //Launch the task
            task.Show();
        }

        private void ShareOnFB_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ShareLinkTask task = new ShareLinkTask();
            task.Title = "Hello World";
            task.LinkUri = new Uri("http://www.google.com", UriKind.Absolute);
            task.Message = "Test";
            task.Show();
        }

        private void ShareBySkyDrive_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }
    }
}