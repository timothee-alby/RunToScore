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
using RunToScore.classes;
using System.Collections.ObjectModel;
using Microsoft.Phone.Shell;
using RunToScore.Resources;

////for debug
//using System.Threading;
//using System.Globalization;
////end debug
namespace RunToScore
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructeur
        public MainPage()
        {
            ////for debug
            //CultureInfo culture = new CultureInfo("en-US");
            //Thread.CurrentThread.CurrentCulture = culture;
            //Thread.CurrentThread.CurrentUICulture = culture;
            ////end debug

            InitializeComponent();
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingGPS"))
            {
                var result = MessageBox.Show(AppResources.AllowGPSText, AppResources.AllowGPS, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                    IsolatedStorageSettings.ApplicationSettings.Add("settingGPS", true);
                else
                    IsolatedStorageSettings.ApplicationSettings.Add("settingGPS", false);
            }
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            BuildApplicationBar();
            // Affichage de l'historique
            if (IsolatedStorageSettings.ApplicationSettings.Contains("runHistory"))
            {
                List<FullRun> runHistory = new List<FullRun>();

                /*DEBUG : Uncomment following line to clear run history*/
                //IsolatedStorageSettings.ApplicationSettings["runHistory"] = runHistory; //This line clear run history
                /*END DEBUG*/

                runHistory = (List<FullRun>)IsolatedStorageSettings.ApplicationSettings["runHistory"];
                List<FullRun> runHistoryClone = runHistory.Select(i => i.Clone()).ToList();

                runHistoryClone.Reverse();
                historyListBox.ItemsSource = runHistoryClone;
            }
            getTotals();
        }
        private void getTotals()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("runHistory"))
            {
                // Calcul des totaux
                int totalPts = 0;
                double totalDist = 0;
                TimeSpan totalDuration = new TimeSpan();
                double sumAvgVitesse = 0;
                double totalAvgVitesse = 0;
                int totalKCal = 0;

                List<FullRun> runHistory = new List<FullRun>();
                runHistory = (List<FullRun>)IsolatedStorageSettings.ApplicationSettings["runHistory"];
                foreach (FullRun fr in runHistory)
                {
                    totalPts += fr.points;
                    totalDist += fr.distance;
                    totalDuration += fr.duration;
                    if(fr.vitesse > 0)
                        sumAvgVitesse += (fr.duration.Hours*3600+fr.duration.Minutes*60+fr.duration.Seconds) * fr.vitesse;
                    totalKCal += fr.kCal;
                }
                totalAvgVitesse = ((totalDuration.Days * 86400 + totalDuration.Hours * 3600 + totalDuration.Minutes * 60 + totalDuration.Seconds) > 0) ? (sumAvgVitesse / (totalDuration.Hours * 3600 + totalDuration.Minutes * 60 + totalDuration.Seconds)) : 0;

                // Affichage des totaux
                nbRun_val.Text = "" + runHistory.Count();
                points_val.Text = "" + totalPts;

                duration_val.Text = "" + (totalDuration.Days * 24 + totalDuration.Hours) + "h "+totalDuration.Minutes+"m "+totalDuration.Seconds+"s";
                kCal_val.Text = "" + totalKCal;

                if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingMetricKilometers"))
                    IsolatedStorageSettings.ApplicationSettings.Add("settingMetricKilometers", true);
                if ((Boolean)IsolatedStorageSettings.ApplicationSettings["settingMetricKilometers"])
                {
                    distance_val.Text = "" + String.Format("{0:0.00}", totalDist / 1000);
                    distance_unit.Text = "km";
                    avgVitesse_val.Text = "" + String.Format("{0:0.00}", totalAvgVitesse);
                    avgVitesse_unit.Text = "km/h";
                }
                else
                {
                    distance_val.Text = "" + String.Format("{0:0.00}", totalDist * 0.000621371192);
                    distance_unit.Text = "mi";
                    avgVitesse_val.Text = "" + String.Format("{0:0.00}", totalAvgVitesse * 0.621371192);
                    avgVitesse_unit.Text = "mph";
                }
               
            }
        }
        private void about_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }
        private void settings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }
        private void newRun_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/run.xaml", UriKind.Relative));
        }
        private void historyListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (historyListBox.SelectedItem == null)
                return;
            FullRun currentRun = (FullRun)historyListBox.SelectedItem;
            PhoneApplicationService.Current.State["currentRunHistory"] = currentRun;
            historyListBox.SelectedItem = null;
            NavigationService.Navigate(new Uri("/History.xaml", UriKind.Relative));
        }
        private void BuildApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarButton1 = new ApplicationBarIconButton(new Uri("/Images/appbar.questionmark.rest.png", UriKind.Relative));
            appBarButton1.Text = AppResources.about;
            appBarButton1.Click += about_Click;
            ApplicationBar.Buttons.Add(appBarButton1);

            ApplicationBarIconButton appBarButton2 = new ApplicationBarIconButton(new Uri("/Images/appbar.feature.settings.rest.png", UriKind.Relative));
            appBarButton2.Text = AppResources.settings;
            appBarButton2.Click += settings_Click;
            ApplicationBar.Buttons.Add(appBarButton2);

            ApplicationBarIconButton appBarButton3 = new ApplicationBarIconButton(new Uri("/Images/appbar.new.rest.png", UriKind.Relative));
            appBarButton3.Text = AppResources.newRun;
            appBarButton3.Click += newRun_Click;
            ApplicationBar.Buttons.Add(appBarButton3);

        }
    }
}