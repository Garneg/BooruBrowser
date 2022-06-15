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



namespace BooruBrowser
{
    public static class FragmentsController
    {
        private static List<AndroidX.Fragment.App.Fragment> fragmentsStack = new List<AndroidX.Fragment.App.Fragment>();

        public static int FragmentsCount { get => fragmentsStack.Count; }

        public static void AddToStack(AndroidX.Fragment.App.Fragment fragment)
        {
            fragmentsStack.Add(fragment);
        }

        public static AndroidX.Fragment.App.Fragment GetLast()
        {
            return fragmentsStack.Count > 0 ? fragmentsStack.Last() : null;
        }

        public static AndroidX.Fragment.App.Fragment GetAndRemoveLast()
        {
            var last = GetLast();

            RemoveLast();

            return last;

        }
        
        public static void RemoveLast()
        {
            fragmentsStack.RemoveAt(fragmentsStack.Count - 1);
        }

    }
}