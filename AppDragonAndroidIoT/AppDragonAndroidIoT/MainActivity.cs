using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Locations;
using System.Collections.Generic;
using Android.Util;
using System.Linq;
using System.Threading.Tasks;

namespace AppDragonAndroidIoT
{
    [Activity(Label = "AppDragonAndroidIoT", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, ILocationListener
    {
        int count = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };

            Button buttonIoT = FindViewById<Button>(Resource.Id.buttonIoT);
            buttonIoT.Click += ButtonIoT_Click;

            InitializeLocationManager();
            InitializeIoTHub();
        }

        private async void ButtonIoT_Click(object sender, EventArgs e)
        {
            if (iotHubClient != null && (!sendingAvailable))
            {
                sendingAvailable = true;
            }
            if (sendingAvailable)
            {
                currentLocation = locationManager.GetLastKnownLocation(locationProvider);
                if (currentLocation != null)
                {
                    var text = FindViewById<TextView>(Resource.Id.textViewLocationStatus);
                    text.Text = "Location - " + currentLocation.ToString();

                    await SendLocation();
                }
            }
        }

        Microsoft.Azure.Devices.Client.DeviceClient iotHubClient;
        string connectionString = "HostName=[IoT Hub Name].azure-devices.net;DeviceId=[DeviceId];SharedAccessKey=[DeviceKey]";
        bool sendingAvailable = false;
        private async void InitializeIoTHub()
        {
            try
            {
                iotHubClient = Microsoft.Azure.Devices.Client.DeviceClient.CreateFromConnectionString(connectionString, Microsoft.Azure.Devices.Client.TransportType.Http1);
                await iotHubClient.OpenAsync();

                string content = "{\"message\":\"Hello from Dragon Board!\",\"Devicetime\":\"" + DateTime.Now.ToString("yyyy/MM/ddTHH:mm:ss") + "\"}";
                await iotHubClient.SendEventAsync(new Microsoft.Azure.Devices.Client.Message(System.Text.Encoding.UTF8.GetBytes(content)));
                sendingAvailable = true;
                ReceiveMessages();
            }
            catch (Exception ex)
            {
                Log.Debug(TAG, "IoT Hub Error +" + ex.Message);
            }
        }

        public async Task ReceiveMessages()
        {
            while (true)
            {
                var msg = await iotHubClient.ReceiveAsync();
                if (msg != null)
                {
                    string content = System.Text.Encoding.UTF8.GetString(msg.GetBytes());
                    Log.Debug(TAG, "Message Recievice - " + content);
                    await iotHubClient.CompleteAsync(msg);
                }
            }
        }

        private async Task SendLocation()
        {
            if (iotHubClient != null && sendingAvailable)
            {
                string content = "{\"longitude\":" + currentLocation.Longitude + ",\"latitude\":" + currentLocation.Latitude + ",\"Devicetime\":\"" + DateTime.Now.ToString("yyyy/MM/ddTHH:mm:ss") + "\"}";
                byte[] buf = System.Text.Encoding.UTF8.GetBytes(content);
                var msg = new Microsoft.Azure.Devices.Client.Message(buf);
                await iotHubClient.SendEventAsync(msg);
            }
        }

        LocationManager locationManager;
        string locationProvider;
        static readonly string TAG = "X:" + typeof(MainActivity).Name;
        Location currentLocation;

        void InitializeLocationManager()
        {
            locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria
            {
               // Accuracy = Accuracy.Fine
               Accuracy=Accuracy.Coarse
            };
            IList<string> acceptableLocationProviders = locationManager.GetProviders(criteriaForLocationService, true);

            if (acceptableLocationProviders.Any())
            {
                locationProvider = acceptableLocationProviders.First();
                locationManager.RequestLocationUpdates(locationProvider, 60000, 100, this);
            }
            else
            {
                locationProvider = string.Empty;
            }
            Log.Debug(TAG, "Using " + locationProvider + ".");
            currentLocation = locationManager.GetLastKnownLocation(locationProvider);
        }

        public async void OnLocationChanged(Location location)
        {
            currentLocation = location;
            if (currentLocation == null)
            {
                //  _locationText.Text = "Unable to determine your location. Try again in a short while.";
            }
            else
            {
                await SendLocation();
                //  _locationText.Text = string.Format("{0:f6},{1:f6}", _currentLocation.Latitude, _currentLocation.Longitude);
                //    Address address = await ReverseGeocodeCurrentLocation();
                //  DisplayAddress(address);
            }

        }

        public void OnProviderDisabled(string provider)
        {
            var text = FindViewById<TextView>(Resource.Id.textViewLocationStatus);
            text.Text = "Disabled - " + provider;
        }

        public void OnProviderEnabled(string provider)
        {
            var text = FindViewById<TextView>(Resource.Id.textViewLocationStatus);
            text.Text = "Enabled - " + provider;
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            var text = FindViewById<TextView>(Resource.Id.textViewLocationStatus);
            text.Text = "Status Changed - " + provider + " - " + status.ToString();
        }

    }
}

