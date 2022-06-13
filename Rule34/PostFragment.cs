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

namespace Rule34
{
    class PostFragment : AndroidX.Fragment.App.Fragment
    {
        private Post post;
        private PostThumbnail thumbnail;
        private Android.Graphics.Bitmap bitmap;
        public PostFragment() : base()
        {

        }

        
        public PostFragment(Android.Graphics.Bitmap bitmap)
        {
            this.bitmap = bitmap;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.post_fragment_layout, container, false);
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            //Activity.FindViewById<ImageView>(Resource.Id.imageView1).SetImageResource(Resource.Drawable.topb);

            if (bitmap!= null)
            {
                //Activity.FindViewById<ImageView>(Resource.Id.imageView1).SetImageBitmap(bitmap);
                
            }
            //Activity.FindViewById<ImageView>(Resource.Id.imageView1).SetBackgroundColor(Android.Graphics.Color.BlanchedAlmond);



        }



    }
}