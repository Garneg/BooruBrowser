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
using System.Net;
using Android.Graphics;
using Android.Support.V7.Widget;


namespace BooruBrowser
{
    internal class SearchRecyclerViewAdapter : Android.Support.V7.Widget.RecyclerView.Adapter
    {
        public override int ItemCount => posts.Length;

        private BooruPost[] posts;
       

        public SearchRecyclerViewAdapter(BooruPost[] booruPosts)
        {
            posts = booruPosts;
            
        }

        public async override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            

            View holderView = holder.ItemView;
            var thumb = holderView.FindViewById<ImageView>(Resource.Id.thumbnail_image);
            var text = holderView.FindViewById<TextView>(Resource.Id.post_recyclerview_item_additonal_text);

            text.Text = String.Join(' ', posts[position].Tags);

            if ((holder as SearchRecyclerViewAdapterViewHolder).bitmap != null)
            {
                thumb.SetImageBitmap((holder as SearchRecyclerViewAdapterViewHolder).bitmap);
                return;
            }

            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;

            WebClient webClient = new WebClient();
            byte[] imageBytes = await webClient.DownloadDataTaskAsync(posts[position].SampleUrl.Contains(".mp4") ? posts[position].PreviewUrl : posts[position].SampleUrl);

            Bitmap thumbBitmap = await BitmapFactory.DecodeByteArrayAsync(imageBytes, 0, imageBytes.Length);

            (holder as SearchRecyclerViewAdapterViewHolder).bitmap = thumbBitmap;

            int height = (int)((float)thumbBitmap.Height / (float)thumbBitmap.Width * (float)holderView.Width);

            thumb.SetMinimumHeight(height);


            thumb.SetImageBitmap(thumbBitmap);
            

        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.post_recyclerview_item, parent, false);


            var holder = new SearchRecyclerViewAdapterViewHolder(view);

            return holder;
        }

        public override void OnViewRecycled(Java.Lang.Object holder)
        {
            var realHolder = holder as SearchRecyclerViewAdapterViewHolder;
            
        }

    }

    internal class SearchRecyclerViewAdapterViewHolder : Android.Support.V7.Widget.RecyclerView.ViewHolder
    {
        public Bitmap bitmap;

        public SearchRecyclerViewAdapterViewHolder(View view) : base(view)
        {
            
        }
    }
}