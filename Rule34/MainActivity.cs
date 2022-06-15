using Android.App;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using Android.Widget;
using Android.Views;
using System;
using System.Net;
using System.IO;
using Android.Graphics;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Xml;
using System.Xml.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Android.InputMethodServices;
using Android.Views.InputMethods;
using System.Collections.Generic;


namespace BooruBrowser
{
    [Activity(Label = "@string/app_name", MainLauncher = true, 
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        SearchFragment searchFragment;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Window.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);
            searchFragment = new SearchFragment();
            var transaction = SupportFragmentManager.BeginTransaction();
            transaction.SetReorderingAllowed(true);
            transaction.Replace(Resource.Id.main_frame_layout, searchFragment);
            transaction.Commit();
            
        }

    }
}