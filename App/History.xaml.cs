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
using RunToScore.classes;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Controls.Maps;
using System.IO.IsolatedStorage;
using RunToScore.Resources;

namespace RunToScore
{
    public partial class History : PhoneApplicationPage
    {
        public History()
        {
            InitializeComponent();
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            BuildApplicationBar();
            var entry = NavigationService.BackStack.FirstOrDefault();
            if (entry != null && entry.Source.OriginalString.Contains("run.xaml"))
            {
                var _RootFrame = (Application.Current as App).RootFrame;
                _RootFrame.RemoveBackEntry();
            }

            FullRun fullRun = (FullRun)PhoneApplicationService.Current.State["currentRunHistory"];
            if (fullRun != null)
            {
                fullRunDate.Text = fullRun.dateFormated;
                fullRunPoints.Text = "" + fullRun.points;
                fullRunKCal.Text = "" + fullRun.kCal;

                fullRunTime.Text = fullRun.duration.Hours + "h" + String.Format("{0:00}", fullRun.duration.Minutes);
                fullRunTime_bottom.Text = String.Format("{0:00}", fullRun.duration.Seconds)+"s";

                if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingMetricKilometers"))
                    IsolatedStorageSettings.ApplicationSettings.Add("settingMetricKilometers", true);
                if ((Boolean)IsolatedStorageSettings.ApplicationSettings["settingMetricKilometers"])
                {
                    fullRunDistance.Text = String.Format("{0:0.00}", fullRun.distance / 1000);
                    fullRunDistance_unit.Text = "km";
                    fullRunVitesse.Text = String.Format("{0:0.00}", fullRun.vitesse);
                    fullRunVitesse_unit.Text = "km/h";
                }
                else
                {
                    fullRunDistance.Text = String.Format("{0:0.00}", fullRun.distance * 0.000621371192);
                    fullRunDistance_unit.Text = "mi";
                    fullRunVitesse.Text = String.Format("{0:0.00}", fullRun.vitesse * 0.621371192);
                    fullRunVitesse_unit.Text = "mph";
                }


                // ROUTE DRAW //
                Color routeColor = ((SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"]).Color;
                Color routeColorBrush = Colors.Blue;
                SolidColorBrush routeBrush = new SolidColorBrush(routeColor);
                
                
                MapPolyline routeLine = new MapPolyline();
                MapLayer myRouteLayer = new MapLayer();
                routeLine.Locations = fullRun.journey;

                routeLine.Stroke = routeBrush;
                routeLine.Opacity = 0.50;
                routeLine.StrokeThickness = 10.0;

                // Add the route line to the new layer.
                myRouteLayer.Children.Add(routeLine);

                // Add a map layer in which to draw the route.
                map1.Children.Add(myRouteLayer);
                if (fullRun.journey.Count() > 0)
                {
                    map1.Center = fullRun.journey[0];
                    map1.ZoomLevel = 15;
                }
            }
            base.OnNavigatedTo(e);
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

            //ApplicationBarIconButton appBarButton2 = new ApplicationBarIconButton(new Uri("/Images/appbar.feature.email.rest.png", UriKind.Relative));
            //appBarButton2.Text = AppResources.settings;
            //appBarButton2.Click += share_Click;
            //ApplicationBar.Buttons.Add(appBarButton2);

            //ApplicationBarIconButton appBarButton3 = new ApplicationBarIconButton(new Uri("/Images/appbar.delete.rest.png", UriKind.Relative));
            //appBarButton3.Text = AppResources.newRun;
            //appBarButton3.Click += delete_Click;
            //ApplicationBar.Buttons.Add(appBarButton3);

        }
        private void back_Click(object sender, EventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
        private void share_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Share.xaml", UriKind.Relative));
        }
        private void delete_Click(object sender, EventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}