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
using System.IO.IsolatedStorage;
using Microsoft.Phone.Shell;
using RunToScore.Resources;

namespace RunToScore
{
    public partial class Settings : PhoneApplicationPage
    {
        public Settings()
        {
            InitializeComponent();
            BuildApplicationBar();
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            //Set the toggle GPS
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingGPS"))
                IsolatedStorageSettings.ApplicationSettings.Add("settingGPS", false);
            FluxToggleGPS.IsChecked = (Boolean)IsolatedStorageSettings.ApplicationSettings["settingGPS"];

            //Set the toggle Lock
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingLock"))
                IsolatedStorageSettings.ApplicationSettings.Add("settingLock", true);
            FluxToggleLock.IsChecked = (Boolean)IsolatedStorageSettings.ApplicationSettings["settingLock"];

            //Set the toggle Town mode
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingTownMode"))
                IsolatedStorageSettings.ApplicationSettings.Add("settingTownMode", true);
            FluxToggle.IsChecked = (Boolean)IsolatedStorageSettings.ApplicationSettings["settingTownMode"];
           
            //Set the radio metric
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingMetricKilometers"))
                IsolatedStorageSettings.ApplicationSettings.Add("settingMetricKilometers", true);
            if ((Boolean)IsolatedStorageSettings.ApplicationSettings["settingMetricKilometers"])
                radioButtonKilometers.IsChecked = true;
            else
                radioButtonMiles.IsChecked = true;
           
            
        }
        private void FluxToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingTownMode"))
                IsolatedStorageSettings.ApplicationSettings.Add("settingTownMode", true);
            else
                IsolatedStorageSettings.ApplicationSettings["settingTownMode"] = true;
        }
        private void FluxToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingTownMode"))
                IsolatedStorageSettings.ApplicationSettings.Add("settingTownMode", false);
            else
                IsolatedStorageSettings.ApplicationSettings["settingTownMode"] = false;
        }
        private void FluxToggleGPS_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingGPS"))
                IsolatedStorageSettings.ApplicationSettings.Add("settingGPS", true);
            else
                IsolatedStorageSettings.ApplicationSettings["settingGPS"] = true;
        }
        private void FluxToggleGPS_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingGPS"))
                IsolatedStorageSettings.ApplicationSettings.Add("settingGPS", false);
            else
                IsolatedStorageSettings.ApplicationSettings["settingGPS"] = false;
        }
        private void FluxToggleLock_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingLock"))
                IsolatedStorageSettings.ApplicationSettings.Add("settingLock", true);
            else
                IsolatedStorageSettings.ApplicationSettings["settingLock"] = true;
        }
        private void FluxToggleLock_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingLock"))
                IsolatedStorageSettings.ApplicationSettings.Add("settingLock", false);
            else
                IsolatedStorageSettings.ApplicationSettings["settingLock"] = false;
        }
        private void radioButtonMiles_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingMetricKilometers"))
                IsolatedStorageSettings.ApplicationSettings.Add("settingMetricKilometers", false);
            else
                IsolatedStorageSettings.ApplicationSettings["settingMetricKilometers"] = false;
        }
        private void radioButtonKilometers_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingMetricKilometers"))
                IsolatedStorageSettings.ApplicationSettings.Add("settingMetricKilometers", true);
            else
                IsolatedStorageSettings.ApplicationSettings["settingMetricKilometers"] = true;
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