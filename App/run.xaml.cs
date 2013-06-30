using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Device.Location;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Controls.Maps.Platform;
using System.Windows.Threading;
using RunToScore.classes;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Net.NetworkInformation;
using RunToScore.Resources;
using System.ServiceModel;
using RunToScore.GeocodeService;

namespace RunToScore
{


public partial class run : PhoneApplicationPage
    {
        // Constructeur
        private GeoCoordinateWatcher geoWatcher;
        private Pushpin actualPositionPushpin;
        //private String BingMapKey = "Am1eKq3vy06AHytpB_ueTbHrhBtYKw4cMdQ8Hosk1QCQLD6knJ75ahoKQLwMhXMV";
        private String BingMapKey = "8Rrx+bKJTjpVFB7Nwr4tKmPAlZ01csOJEhU7x/S8TEc=";
        Location currentLocation;
        Location lastLocation100m;
        Location lastLocation5m;
        MapPolyline routeLine = new MapPolyline();
        LocationCollection journey = new LocationCollection();
        MapLayer myRouteLayer = new MapLayer();
        FullRun fullRun;

        private Boolean isRunStarted = false;

        //Données de course
        private double vitesseSum = 0;
        private double vitesseNbforAvg = 0;
        private double totalDistance = 0;
        private int totalPoints = 0;
        DispatcherTimer dt;

        private Boolean isTownModeEnabled = true;

        private Boolean isGPSStatutOK = false;

        private Boolean isPhoneObscured = false;
        public run()
        {
            InitializeComponent();
            //Initiate GPS if user allow it
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingGPS"))
                IsolatedStorageSettings.ApplicationSettings.Add("settingGPS", false);
            if ((Boolean)IsolatedStorageSettings.ApplicationSettings["settingGPS"])
            {
                geoWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
                geoWatcher.StatusChanged += geoWatcher_StatusChanged;
                geoWatcher.MovementThreshold = 2.5f;
                geoWatcher.PositionChanged += watcher_PositionChanged;
                geoWatcher.Start();
            }
            else
            {
                startText.Text = AppResources.PleaseAllowGPS;
                startText.Tap += goBack;
            }
            //Disable idle detection to run under lock screen (if user allow it)
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingLock"))
                IsolatedStorageSettings.ApplicationSettings.Add("settingLock", true);
            if ((Boolean)IsolatedStorageSettings.ApplicationSettings["settingLock"])
            {
                PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
                var _RootFrame = (Application.Current as App).RootFrame;
                _RootFrame.Obscured += rootFrameObscured;
                _RootFrame.Unobscured += rootFrameUnObscured;
            }

            //Getting Town Mode
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingTownMode"))
                IsolatedStorageSettings.ApplicationSettings.Add("settingTownMode", true);
            isTownModeEnabled = (Boolean)IsolatedStorageSettings.ApplicationSettings["settingTownMode"];


            // ROUTE DRAW //
            Color routeColor = ((SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"]).Color;
            Color routeColorBrush = Colors.Blue;
            SolidColorBrush routeBrush = new SolidColorBrush(routeColor);
            routeLine.Stroke = routeBrush;
            routeLine.Opacity = 0.50;
            routeLine.StrokeThickness = 10.0;

            routeLine.Locations = new LocationCollection();
            routeLine.Stroke = routeBrush;

            // Add the route line to the new layer.
            myRouteLayer.Children.Add(routeLine);

            // Add a map layer in which to draw the route.
            map1.Children.Add(myRouteLayer);
            map1.ZoomLevel = 15;
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settingMetricKilometers"))
                IsolatedStorageSettings.ApplicationSettings.Add("settingMetricKilometers", true);
            if ((Boolean)IsolatedStorageSettings.ApplicationSettings["settingMetricKilometers"])
            {
                fullRunDistance_unit.Text = "km";
                fullRunVitesse_unit.Text = "km/h";
            }
            else
            {
                fullRunDistance_unit.Text = "mi";
                fullRunVitesse_unit.Text = "mph";
            }
        }
        private void updateDistance()
        {
            if (!isPhoneObscured)
            {
                if ((Boolean)IsolatedStorageSettings.ApplicationSettings["settingMetricKilometers"])
                    fullRunDistance.Text = String.Format("{0:0.00}", totalDistance / 1000);
                else
                    fullRunDistance.Text = String.Format("{0:0.00}", totalDistance * 0.000621371192);
            }
        }
        private void updateVitesse(double _vitesse)
        {
            if (!isPhoneObscured)
            {
                if ((Boolean)IsolatedStorageSettings.ApplicationSettings["settingMetricKilometers"])
                    fullRunVitesse.Text = String.Format("{0:0.00}", _vitesse);
                else
                    fullRunVitesse.Text = String.Format("{0:0.00}", _vitesse * 0.621371192);
            }
        }
        private void updateTime()
        {
            TimeSpan time = DateTime.UtcNow.Subtract(fullRun.startDate);
            if (!isPhoneObscured)
            {
                fullRunTime.Text = time.Hours + ":" + String.Format("{0:00}", time.Minutes);
                fullRunTime_bottom.Text = String.Format("{0:00}", time.Seconds);
            }
            //if (time.Hours <= 0)
            //    fullRunTime.Text = String.Format("{0:00}", time.Minutes) + ":" + String.Format("{0:00}", time.Seconds);
            //else
            //    fullRunTime.Text = String.Format("{0:00}", time.Hours) + ":" + String.Format("{0:00}", time.Minutes);
        }
        private void geoWatcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    // le gps n'est pas activé
                    isGPSStatutOK = false;
                    break;
                case GeoPositionStatus.Initializing:
                    // le gps est en cours d'initialisation
                    isGPSStatutOK = false;
                    break;
                case GeoPositionStatus.NoData:
                    // il n'y a pas d'informations
                    isGPSStatutOK = false;
                    break;
                case GeoPositionStatus.Ready:
                    // le GPS est activé et disponible
                    isGPSStatutOK = true;
                    break;
            }
            if (actualPositionPushpin != null)
            {
                ImageBrush ib = new ImageBrush();
                if (isGPSStatutOK)
                    ib.ImageSource = new BitmapImage(new Uri("/Images/globe-icone-48.png", UriKind.Relative));
                else
                    ib.ImageSource = new BitmapImage(new Uri("/Images/globeYellow-icone-48.png", UriKind.Relative));
                actualPositionPushpin.Content = new Ellipse()
                {
                    Fill = ib,
                    Height = 30,
                    Width = 30,
                    Opacity = .6
                };
            }
        }
        private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (isGPSStatutOK)
                {
                    //Get the current location and center on it
                    currentLocation = e.Position.Location;
                    map1.Center = e.Position.Location;

                    //update (or create) the pushpin for current location
                    if (map1.Children.OfType<Pushpin>().Any())
                        actualPositionPushpin.Location = e.Position.Location;
                    else
                    {
                        actualPositionPushpin = new Pushpin { Location = currentLocation };
                        actualPositionPushpin.Name = "actualPositionPushpin";

                        //changing pushpin image
                        actualPositionPushpin.Template = null;
                        ImageBrush ib = new ImageBrush();
                        if(isGPSStatutOK)
                            ib.ImageSource = new BitmapImage(new Uri("/Images/globe-icone-48.png", UriKind.Relative));
                        else
                            ib.ImageSource = new BitmapImage(new Uri("/Images/globeYellow-icone-48.png", UriKind.Relative));

                        actualPositionPushpin.PositionOrigin = PositionOrigin.Center;
                        actualPositionPushpin.Content = new Ellipse()
                        {
                            Fill = ib,
                            Height = 30,
                            Width = 30,
                            Opacity = .6
                        };
                        map1.Children.Add(actualPositionPushpin);

                        //routeLine.Locations.Add(currentLocation);
                    }
                    if (isRunStarted)
                    {
                        //Set vitesse et vitesse max
                        double vit = double.IsNaN(e.Position.Location.Speed) ? 0 : (e.Position.Location.Speed / 1000 * 3600);
                        updateVitesse(vit);
                        //if (vit > vitesseMax)
                        //    vitesseMax = vit;
                        //vitesse.Text = String.Format("{0:0.00}", vitesseSum / vitesseNbforAvg);
                        //vitesseMaxi.Text = String.Format("{0:0.00}", vitesseMax);

                        //Actions appenning when a certain distance is pass
                        int dist = 0;
                        //More than 5m : add distance, add routeline and watch for pushpin nearby. Also record the vitesse to display AVG vitesse
                        if (lastLocation5m == null)
                            lastLocation5m = currentLocation;
                        else
                        {
                            dist = distanceBetweenLocations(currentLocation, lastLocation5m);
                            if (dist >= 5 && dist < 100)
                            {
                                totalDistance += dist;
                                updateDistance();
                                lastLocation5m = currentLocation;
                                routeLine.Locations.Add(currentLocation);
                                vitesseSum += vit;
                                vitesseNbforAvg += 1;
                                lookForPushPinNearBy();
                            }
                        }
                        //More than 100m : add pushpins
                        if (lastLocation100m == null)
                        {
                            lastLocation100m = currentLocation;
                            addPushPinAround();
                        }
                        else
                        {
                            dist = distanceBetweenLocations(currentLocation, lastLocation100m);
                            if (dist >= 100 && dist < 500)
                            {
                                addPushPinAround();
                                lastLocation100m = currentLocation;
                                journey.Add(currentLocation);
                            }
                        }
                    }
                }
            });
        }
        private void addPushPinAround()
        {
            if (currentLocation != null)
            {
                for (double i = -0.002; i <= 0.002; i += 0.001)
                {
                    for (double j = -0.002; j <= 0.002; j += 0.001)
                    {
                        if (i != 0.0 && j != 0.0)
                        {
                            Random random = new Random();
                            //int rand = random.Next(3);
                            double randForLocation = random.Next(10);
                            //if(rand==0)
                            if (DeviceNetworkInformation.IsNetworkAvailable && isTownModeEnabled)
                                getAddressFromPoint(currentLocation.Latitude + i * 3 + randForLocation / 10000, currentLocation.Longitude + j * 3 + randForLocation / 10000);
                            else
                                addPushPinToLocation(currentLocation.Latitude + i * 3 + randForLocation / 1000, currentLocation.Longitude + j * 3 + randForLocation / 1000);
                            //getAddressFromPoint(currentLocation.Latitude + i * 3, currentLocation.Longitude + j * 3);
                        }
                    }
                }
            }
        }
        private void lookForPushPinNearBy()
        {
            Array pushpinArray = map1.Children.OfType<Pushpin>().ToArray();
            foreach (Pushpin pin in pushpinArray)
            {
                int dist = distanceBetweenLocations(pin.Location, currentLocation);
                if (pin.Name != "actualPositionPushpin" && dist <= 50)
                    pushpinReached(pin);
                else if(pin.Name != "actualPositionPushpin" && dist > 1000)
                    map1.Children.Remove(pin);
            }
        }
        private void pushpinReached(Pushpin pin)
        {
            if (totalPoints < 800)
            {
                totalPoints += 1;
                if(!isPhoneObscured)
                    fullRunPoints.Text = "" + totalPoints;
            }
            map1.Children.Remove(pin);
        }
        private int distanceBetweenLocations(Location location1, Location location2)
        {
            double d;
            GeoCoordinate geoCoord1 = new GeoCoordinate(location1.Latitude, location1.Longitude);
            GeoCoordinate geoCoord2 = new GeoCoordinate(location2.Latitude, location2.Longitude);
            d = geoCoord1.GetDistanceTo(geoCoord2);

            return (int)d;
        }
        private void getAddressFromPoint(double latitude, double longitude)
        {
            ReverseGeocodeRequest reverseGeocodeRequest = new ReverseGeocodeRequest()
            {
                Credentials = new Credentials() { ApplicationId = BingMapKey },
                Location = new Location() { Latitude = latitude, Longitude = longitude }
            };
            GeocodeService.GeocodeServiceClient geocodeService = new GeocodeService.GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
            try
            {
                geocodeService.ReverseGeocodeAsync(reverseGeocodeRequest);
                geocodeService.ReverseGeocodeCompleted += geocodeService_ReverseGeocodeCompleted;
            }
            catch (EndpointNotFoundException e)
            {
                geocodeService.Abort();
            }
        }
        private void geocodeService_ReverseGeocodeCompleted(object sender, GeocodeService.ReverseGeocodeCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null && e.Result.Results.Count() > 0)
                {
                    GeocodeResult result = e.Result.Results[0];
                    String formattedAddress = result.Address.FormattedAddress;
                    getPointFromAddress(formattedAddress);
                }
            }
            catch (Exception )
            {
                
                throw ;
            }
        }
        private void getPointFromAddress(String formattedAddress)
        {
            GeocodeRequest geocodeRequest = new GeocodeRequest()
            {
                Credentials = new Credentials() { ApplicationId = BingMapKey },
                Query = formattedAddress,
                Options = new GeocodeOptions() {Count = 1 }
            };
            GeocodeService.GeocodeServiceClient geocodeService = new GeocodeService.GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
            try
            {
                geocodeService.GeocodeAsync(geocodeRequest);
                geocodeService.GeocodeCompleted += geocodeService_GeocodeCompleted;
            }
            catch (EndpointNotFoundException e)
            {
                geocodeService.Abort();
            }
        }
        private void geocodeService_GeocodeCompleted(object sender, GeocodeService.GeocodeCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null && e.Result.Results.Count() > 0)
                {
                    GeocodeResult result = e.Result.Results[0];
                    addPushPinToLocation(result.Locations[0]);
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }
        private void addPushPinToLocation(Location location)
        {
            Pushpin pin = new Pushpin { Location = location};

            //changing pushpin image
            pin.Template = null;
            ImageBrush ib = new ImageBrush();

            String medalUri;

            medalUri = "/Images/medal-gold-32.png";

            /* WORK IN PROGRESS: CHOOSE AN IMAGE.*/
            //Random random = new Random();
            //int rand = random.Next(3);
            //switch (rand)
            //{
            //    case 1:
            //        medalUri = "/Images/medal-bronze-32.png";
            //        break;
            //    case 2:
            //        medalUri = "/Images/medal-silver-32.png";
            //        break;
            //    default:
            //        medalUri = "/Images/medal-gold-32.png";
            //        break;
            //}

            ib.ImageSource = new BitmapImage(new Uri(medalUri, UriKind.Relative));
            pin.PositionOrigin = PositionOrigin.Center;
            pin.Content = new Ellipse()
            {
                Fill = ib,
                Height = 32,
                Width = 32,
                Opacity = .8
            };
            map1.Children.Add(pin);
        }
        private void addPushPinToLocation(double latitude,double longitude)
        {
            Location location = new Location { Longitude = longitude, Latitude = latitude };
            addPushPinToLocation(location);
        }
        private void Start_Click(object sender, EventArgs e)
        {
            if (!isRunStarted)
            {
                fullRun = new FullRun();
                fullRun.startDate = DateTime.UtcNow;

                GridClickToStart.Margin = new Thickness(1000,0,0,0);
                dt = new DispatcherTimer();
                dt.Interval = TimeSpan.FromMilliseconds(100);
                dt.Tick += delegate { updateTime(); };
                dt.Start();

                isRunStarted = true;
            }
        }
        private void Stop_Click(object sender, EventArgs e)
        {
            if (isRunStarted)
            {
                isRunStarted = false;
                dt.Stop();
                //Save the data
                fullRun.endDate = DateTime.UtcNow;
                fullRun.distance = totalDistance;
                fullRun.points = totalPoints;
                fullRun.vitesse = (vitesseNbforAvg>0)?(vitesseSum / vitesseNbforAvg):0;
                fullRun.journey = journey;

                addFullRunToIsolatedStorage(true);
            }
            else
            {
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
        }
        private void addFullRunToIsolatedStorage(bool goToHistory)
        {
            List<FullRun> runHistory = new List<FullRun>();
            if (IsolatedStorageSettings.ApplicationSettings.Contains("runHistory"))
            {
                runHistory = (List<FullRun>)IsolatedStorageSettings.ApplicationSettings["runHistory"];
                runHistory.Add(fullRun);
                IsolatedStorageSettings.ApplicationSettings["runHistory"] = runHistory;
            }
            else
            {
                runHistory.Add(fullRun);
                IsolatedStorageSettings.ApplicationSettings.Add("runHistory", runHistory);
            }
            if (goToHistory)
            {
                PhoneApplicationService.Current.State["currentRunHistory"] = fullRun;
                NavigationService.Navigate(new Uri("/History.xaml", UriKind.Relative));
            }
        }
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (isRunStarted)
            {
                var result = MessageBox.Show(AppResources.StopReccordingsText, AppResources.StopReccordings, MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK)
                {
                    e.Cancel = true;
                }
            }
        }
        private void rootFrameObscured(object sender, ObscuredEventArgs e)
        {
            dt.Stop();
            isPhoneObscured = true;
        }
        private void rootFrameUnObscured(object sender, EventArgs e)
        {
            dt.Start();
            isPhoneObscured = false;
            fullRunPoints.Text = "" + totalPoints;
        }
        private void goBack(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }
    }
}