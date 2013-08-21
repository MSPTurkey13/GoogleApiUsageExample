using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GoogleApiUsage.Phone.Resources;
using System.IO.IsolatedStorage;
using Windows.Devices.Geolocation;

namespace GoogleApiUsage.Phone
{
    public class Location
    {
        #region Properties

        // enlem
        public string Latitude { get; set; }
        // boylam
        public string Longtitude { get; set; }

        #endregion
    }

    public partial class MainPage : PhoneApplicationPage
    {
        public Location currentLocation { get; set; }
        public WebClient client;
        private static string GoogleAPIKey = "XXX";

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            currentLocation = new Location();
            client = new WebClient();
        }

        private Uri CreateHotelRequestUri()
        {
            string url = string.Format("https://maps.googleapis.com/maps/api/place/radarsearch/json?location={0},{1}&radius=5000&types=lodging&sensor=false&key={2}",
                currentLocation.Latitude,
                currentLocation.Longtitude,
                GoogleAPIKey
                );
            return new Uri(url);
        }

        // google api usage for hotels
        private void GetHotelDataFromGoogle()
        {
            txtHotelResult.Text = "Loading hotel results ...";
            client.DownloadStringCompleted += client_DownloadStringCompleted;
            client.DownloadStringAsync(CreateHotelRequestUri());
        }

        // download tamamlandığında veri gelmiş olacak
        private void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string jsonResult = e.Result;

            // json verisi işlenerek gerekli veri bir listeye alınıp ekrana bastırılabilir

            txtHotelResult.Text = jsonResult;
        }

        // get location button clicked
        private async void btnGetLocation_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            {
                // The user has opted out of Location.
                return;
            }

            txtLocation.Text = "Getting your location info ...";

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            try
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10)
                    );

                currentLocation.Latitude = geoposition.Coordinate.Latitude.ToString().Replace(",", ".");
                currentLocation.Longtitude = geoposition.Coordinate.Longitude.ToString().Replace(",", ".");

                txtLocation.Text = string.Format("Latitude: {0} Longtitude: {1}", currentLocation.Latitude, currentLocation.Longtitude);

                GetHotelDataFromGoogle();
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    // the application does not have the right capability or the location master switch is off
                    txtLocation.Text = "location  is disabled in phone settings.";
                }
                //else
                {
                    // something else happened acquring the location
                }
            }
        }

        // izin isteme işlemi
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
            {
                // User has opted in or out of Location
                return;
            }
            else
            {
                MessageBoxResult result =
                    MessageBox.Show("This app accesses your phone's location. Is that ok?",
                    "Location",
                    MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
                }

                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

    }
}