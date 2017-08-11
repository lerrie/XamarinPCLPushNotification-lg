using System;

using Android.App;
using Android.Content.PM;
using Android.OS;
using Gcm.Client;
using Android.Util;
using Android.Content;

namespace XamarinPCLPushNotification.Droid
{
    [Activity(Label = "XamarinPCLPushNotification", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            string text = Intent.GetStringExtra("MyData") ?? "Data not available";
            if (text == "MainActivity")
            {
                ShowAlert();                
            }

            LoadApplication(new App());
            RegisterWithGCM();
        }

        private void RegisterWithGCM()
        {
            try
            {


                // Check to ensure everything's set up right
                GcmClient.CheckDevice(this);
                GcmClient.CheckManifest(this);

                // Register for push notifications
                Log.Info("MainActivity", "Registering...");

                PushHandlerService.Context = this;

                //GcmClient.Register(this, App.client.GoogleAPISenderID);
                GcmClient.Register(this, "517660205040");



            }
            catch (Java.Net.MalformedURLException)
            {
                CreateAndShowDialog("There was an error creating the client. Verify the URL.", "Notification Error");
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e.Message, "Notification Error");
            }
        }

        private void CreateAndShowDialog(String message, String title)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.Create().Show();
        }

        /// <summary>
        /// Shows the alert for notification.
        /// </summary>
        /// <returns>The notification alert.</returns>
        public void ShowAlert()
        {
            var prefs = Xamarin.Forms.Forms.Context.GetSharedPreferences(Xamarin.Forms.Forms.Context.PackageName, FileCreationMode.Private);
            string nmsg = prefs.GetString("NotificationMessage", "");
            string ntitle = prefs.GetString("NotificationTitle", "");

            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle(ntitle);
            alert.SetMessage(nmsg);
            alert.SetPositiveButton("OK", (senderAlert, args) =>
            {
                alert.Dispose();
            });

            RunOnUiThread(() =>
            {
                alert.Show();
            });
        }

    }
}

