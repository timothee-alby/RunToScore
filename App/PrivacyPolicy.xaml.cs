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
using Microsoft.Phone.Shell;
using RunToScore.Resources;

namespace RunToScore
{
    public partial class PrivacyPolicy : PhoneApplicationPage
    {
        public PrivacyPolicy()
        {
            InitializeComponent();
            BuildApplicationBar();
        }
        private void BuildApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarButton1 = new ApplicationBarIconButton(new Uri("/Images/appbar.back.rest.png", UriKind.Relative));
            appBarButton1.Text = AppResources.about;
            appBarButton1.Click += back_Click;
            ApplicationBar.Buttons.Add(appBarButton1);
        }
        private void back_Click(object sender, EventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}