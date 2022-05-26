using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndroidX.AppCompat.App;



namespace Rule34
{
    public static class FragmentsController
    {
        public static bool IsInitialized { get; private set; } = false;

        static SearchFragment searchFragment;
        static PostFragment postFragment;

        public static void Initialize()
        {
            searchFragment = new SearchFragment();
            postFragment = new PostFragment();
        }

        public static void SwitchToSearchFragment()
        {
            
        }
        

    }
}