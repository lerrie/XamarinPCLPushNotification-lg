using Android.App;
using Android.Content;
using Android.Util;
using Gcm.Client;
using System.Collections.Generic;
using WindowsAzure.Messaging;
using Java.Lang;
using Android.Media;

[assembly: Permission(Name = "com.rciapps.camobile.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "com.rciapps.camobile.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "com.google.android.c2dm.permission.RECEIVE")]

//GET_ACCOUNTS is needed only for Android versions 4.0.3 and below
[assembly: UsesPermission(Name = "android.permission.GET_ACCOUNTS")]
[assembly: UsesPermission(Name = "android.permission.INTERNET")]
[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]

[assembly: UsesPermission(Android.Manifest.Permission.ReceiveBootCompleted)]


namespace XamarinPCLPushNotification.Droid
{
    //You must subclass this!
    [BroadcastReceiver(Permission = Gcm.Client.Constants.PERMISSION_GCM_INTENTS)]
    [IntentFilter(new[] { Android.Content.Intent.ActionBootCompleted })] // Allow GCM on boot and when app is closed
    [IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_MESSAGE }, Categories = new string[] { "com.rciapps.camobile" })]
    [IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_REGISTRATION_CALLBACK }, Categories = new string[] { "com.rciapps.camobile" })]
    [IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_LIBRARY_RETRY }, Categories = new string[] { "com.rciapps.camobile" })]


    public class MyBroadcastReceiver : GcmBroadcastReceiverBase<PushHandlerService>
    {
        //public static string[] SENDER_IDS = new string[] { App.client.GoogleAPISenderID };
        public static string[] SENDER_IDS = new string[] { "517660205040" };
        public const string TAG = "MyBroadcastReceiver-GCM";
    }


    [Service] // Must use the service tag
    public class PushHandlerService : GcmServiceBase
    {
        public static string RegistrationID { get; private set; }
        private NotificationHub Hub { get; set; }
        public static Context Context;

        //public PushHandlerService() : base(App.client.GoogleAPISenderID)
        public PushHandlerService() : base("517660205040")
        {
            Log.Info(MyBroadcastReceiver.TAG, "PushHandlerService() constructor");
        }

        protected override void OnRegistered(Context context, string registrationId)
        {
            Log.Verbose(MyBroadcastReceiver.TAG, "GCM Registered: " + registrationId);

            RegistrationID = registrationId;

            //RCI-Lerrie 3.16.17 doesn't need to push notify the user.
            //createNotification("PushHandlerService-GCM Registered...",
            //                    "The device has been Registered!");

            //Hub = new NotificationHub(App.client.NotiHubPath, App.client.NotiHubConnectionStringListen,
            //                            context);
            Hub = new NotificationHub("RCICAMobileNotificationHub", "Endpoint=sb://rcicamobilenotificationnamespace.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=JRkPcobvvJZzpw6ifQXx5czgNKXj3yjQaM33p+65wls=",
                                        context);

            try
            {
                Hub.UnregisterAll(registrationId);
            }
            catch (Exception ex)
            {
                Log.Error(MyBroadcastReceiver.TAG, ex.Message);
            }

            //var tags = new List<string>() { "falcons" }; // create tags if you want
            var tags = new List<string>() { };

            try
            {
                var hubRegistration = Hub.Register(registrationId, tags.ToArray());
            }
            catch (Exception ex)
            {
                Log.Error(MyBroadcastReceiver.TAG, ex.Message);
            }
        }



        protected override void OnMessage(Context context, Intent intent)
        {
            Log.Info(MyBroadcastReceiver.TAG, "GCM Message Received!");

            //var title = string.Format("{0} CA Update", App.client.ClientName);
            var title = string.Format("{0} CA Update", "RCIDEMO");

            //RCI-Lerrie 8/10/17 NEW START
            //-------------------------

            var msg = new Dictionary<string, string>();

            if (intent != null && intent.Extras != null)
            {
                foreach (var key in intent.Extras.KeySet())
                {
                    msg[key] = intent.Extras.Get(key).ToString();
                    System.Console.WriteLine("Key " + intent.Extras.Get(key).ToString());
                }
            }

            string Message = msg["message"];

            //Store the message
            var prefs = GetSharedPreferences(context.PackageName, FileCreationMode.Private);
            var edit = prefs.Edit();
            edit.PutString("last_msg", msg.ToString());
            edit.PutString("NotificationMessage", Message);
            edit.PutString("NotificationTitle", title);
            edit.Commit();
            if (!string.IsNullOrEmpty(Message))
            {
                createNotification(title, Message);
            }

            //-------------------------
            //RCI-Lerrie 8/10/17 NEW END


            //RCI-Lerrie 8/10/17 OLD START
            //-------------------------

            ////var msg = new StringBuilder();

            ////////if (intent != null && intent.Extras != null)
            ////////{
            ////////    foreach (var key in intent.Extras.KeySet())
            ////////        msg.AppendLine(key + "=" + intent.Extras.Get(key).ToString());
            ////////}

            //string messageText = intent.Extras.GetString("message");
            //if (!string.IsNullOrEmpty(messageText))
            //{
            //    createNotification(title, messageText);
            //}
            ////RCI-Lerrie 3.16.17 do not notify unless from azure notification hub
            ////else
            ////{
            ////    createNotification("Unknown message details", msg.ToString());
            ////}

            //-------------------------
            //RCI-Lerrie 8/10/17 OLD END

        }

        void createNotification(string title, string desc)
        {

            //RCI-Lerrie 8/10/17 NEW START
            //-------------------------


            #region Perfrences
            var prefs = Xamarin.Forms.Forms.Context.GetSharedPreferences(Xamarin.Forms.Forms.Context.PackageName, FileCreationMode.Private);
            string nmsg = prefs.GetString("NotificationMessage", "");
            string ntitle = prefs.GetString("NotificationTitle", "");

            #endregion

            var UI_Intent = new Intent(this, typeof(MainActivity));
            UI_Intent.PutExtra("MyData", "MainActivity");

            Android.App.TaskStackBuilder stackBuilder = Android.App.TaskStackBuilder.Create(this);
            stackBuilder.AddParentStack(Java.Lang.Class.FromType(typeof(MainActivity)));
            stackBuilder.AddNextIntent(UI_Intent);

            PendingIntent resultPendingIntent = stackBuilder.GetPendingIntent(0, PendingIntentFlags.UpdateCurrent);
            Notification.Builder builder = new Notification.Builder(this)
            .SetAutoCancel(true) // Remove the notification once the user touches it
            .SetContentTitle(ntitle)
            .SetTicker(ntitle)
            .SetContentIntent(resultPendingIntent)
            .SetContentText(nmsg)
            .SetSmallIcon(Droid.Resource.Drawable.icon)
            .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification));

            Notification notifications = builder.Build();
            NotificationManager notificationsManager = GetSystemService(Context.NotificationService) as NotificationManager;
            notificationsManager.Notify(0, notifications);


            //-------------------------
            //RCI-Lerrie 8/10/17 NEW END


            //RCI-Lerrie 8/10/17 OLD START
            //-------------------------

            ////Create notification
            //var notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;

            ////Create an intent to show ui
            //Intent uiIntent = new Intent(this, typeof(MainActivity));

            ////START Update RCI-Lerrie 7/21/17
            ////change context field to use instead of this

            ////Use Notification Builder

            ////new
            //NotificationCompat.Builder builder = new NotificationCompat.Builder(this);

            ////old
            ////NotificationCompat.Builder builder = new NotificationCompat.Builder(this);


            ////Create the notification
            ////we use the pending intent, passing our ui intent over which will get called
            ////when the notification is tapped.

            ////new
            //var notification = builder.SetContentIntent(PendingIntent.GetActivity(this, 0, uiIntent, 0))
            //        .SetSmallIcon(Resource.Drawable.icon)
            //        .SetTicker(title)
            //        .SetContentTitle(title)
            //        .SetContentText(desc)

            //        //Set the notification sound
            //        .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))

            //        //Auto cancel will remove the notification once the user touches it
            //        .SetAutoCancel(true).Build();

            ////old
            ////var notification = builder.SetContentIntent(PendingIntent.GetActivity(this, 0, uiIntent, 0))
            ////        .SetSmallIcon(Resource.Drawable.icon)
            ////        .SetTicker(title)
            ////        .SetContentTitle(title)
            ////        .SetContentText(desc)

            ////        //Set the notification sound
            ////        .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))

            ////        //Auto cancel will remove the notification once the user touches it
            ////        .SetAutoCancel(true).Build();

            ////END Update RCI-Lerrie 7/21/17


            ////Show the notification
            //notificationManager.Notify(1, notification);
            ////dialogNotify(title, desc, context);

            //-------------------------
            //RCI-Lerrie 8/10/17 OLD END

        }



        /**
        void createNotification(string title, string desc)
        {

            //Create notification
            var notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;

            //Create an intent to show UI
            var uiIntent = new Intent(this, typeof(MainActivity));

            //Create the notification
            var notification = new Notification(Resource.Drawable.icon, title);

            //Auto-cancel will remove the notification once the user touches it
            notification.Flags = NotificationFlags.AutoCancel;

            //Set the notification info
            //we use the pending intent, passing our ui intent over, which will get called
            //when the notification is tapped.
            notification.SetLatestEventInfo(this, title, desc, PendingIntent.GetActivity(this, 0, uiIntent, 0));

            //Show the notification
            notificationManager.Notify(1, notification);
            dialogNotify(title, desc);

        }
            //
    */

        //protected void dialogNotify(string title, string message)
        //{

        //    MainActivity.instance.RunOnUiThread(() => {
        //        AlertDialog.Builder dlg = new AlertDialog.Builder(MainActivity.instance);
        //        AlertDialog alert = dlg.Create();
        //        alert.SetTitle(title);
        //        alert.SetButton("Ok", delegate
        //        {
        //            alert.Dismiss();
        //        });
        //        alert.SetMessage(message);
        //        alert.Show();
        //    });
        //}

        //protected void dialogNotify(string title, string message, Context context)
        //{

        //    AlertDialog.Builder builder = new AlertDialog.Builder(context);

        //    builder.SetMessage(message);
        //    builder.SetTitle(title);
        //    builder.Create().Show();

        //}

        //protected void dialogNotify(string title, string message)
        //{

        //    Android.App.FragmentTransaction ft = FragmentManager.BeginTransaction();
        //    //Remove fragment else it will crash as it is already added to backstack
        //    Android.App.Fragment prev = FragmentManager.FindFragmentByTag("dialog");
        //    if (prev != null)
        //    {
        //        ft.Remove(prev);
        //    }

        //    ft.AddToBackStack(null);

        //    // Create and show the dialog.
        //    MyDialog newFragment = MyDialog.NewInstance(null);

        //    //Add fragment
        //    newFragment.Show(ft, "dialog");

        //}

        protected override void OnUnRegistered(Context context, string registrationId)
        {
            Log.Verbose(MyBroadcastReceiver.TAG, "GCM Unregistered: " + registrationId);

            //var hub = new NotificationHub(Constants.NotiHubPath, Constants.NotiHubConnectionStringListen, context);
            //hub.UnregisterAll(registrationId);

            //RCI-Lerrie 3.16.17 do not need to push notify the user.
            //createNotification("GCM Unregistered...", "The device has been unregistered!");
        }

        protected override bool OnRecoverableError(Context context, string errorId)
        {
            Log.Warn(MyBroadcastReceiver.TAG, "Recoverable Error: " + errorId);

            return base.OnRecoverableError(context, errorId);
        }

        protected override void OnError(Context context, string errorId)
        {
            Log.Error(MyBroadcastReceiver.TAG, "GCM Error: " + errorId);
        }




    }





}
