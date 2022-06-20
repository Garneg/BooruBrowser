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
        private Bitmap[] cachedPreviews;


        public SearchRecyclerViewAdapter(BooruPost[] booruPosts)
        {
            posts = booruPosts;
            cachedPreviews = new Bitmap[posts.Length];
        }

        public async override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {

            var realHolder = (holder as SearchRecyclerViewAdapterViewHolder);
            realHolder.Post = posts[position];

            int holderRecycled = realHolder.RecycleCount;

            View holderView = holder.ItemView;
            var thumb = holderView.FindViewById<ImageView>(Resource.Id.thumbnail_image);
            var text = holderView.FindViewById<TextView>(Resource.Id.post_recyclerview_item_additonal_text);

            int height = (int)((float)posts[position].SampleHeight / (float)posts[position].SampleWidth * (float)(holder as SearchRecyclerViewAdapterViewHolder).ParentWidth);
            thumb.SetMinimumHeight(height);
            thumb.Visibility = ViewStates.Invisible;

            text.Text = String.Join(' ', posts[position].Tags);
            text.SetMaxLines(2);
            text.Click += (s, e) =>
            {
                if (text.MaxLines == 2)
                    text.SetMaxLines(1000);
                else
                    text.SetMaxLines(2);
            };

            if (cachedPreviews[position] != null)
            {
                thumb.SetImageBitmap(cachedPreviews[position]);
                thumb.Visibility = ViewStates.Visible;
            }
            

            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;

            WebClient webClient = new WebClient();
            string url = posts[position].SampleUrl.Contains(".mp4") ? posts[position].PreviewUrl : posts[position].SampleUrl;


            if (cachedPreviews[position] == null)
            {
                byte[] previewBytes = await webClient.DownloadDataTaskAsync(posts[position].PreviewUrl);
                cachedPreviews[position] = await BitmapFactory.DecodeByteArrayAsync(previewBytes, 0, previewBytes.Length);
                if (holderRecycled == realHolder.RecycleCount)
                thumb.SetImageBitmap(cachedPreviews[position]);
            }

            byte[] imageBytes = await webClient.DownloadDataTaskAsync(url);
            Bitmap thumbBitmap = await BitmapFactory.DecodeByteArrayAsync(imageBytes, 0, imageBytes.Length);

            if (holderRecycled == realHolder.RecycleCount)
            {
                thumb.SetImageBitmap(thumbBitmap);
                thumb.Visibility = ViewStates.Visible;
            }

            holderView.Click += (s, e) => HolderClick.Invoke(this, holder);

            holderView.LongClick += (s, e) => HolderLongClick.Invoke(this, holder);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.post_recyclerview_item, parent, false);



            var holder = new SearchRecyclerViewAdapterViewHolder(view)
            {
                ParentWidth = parent.Width
            };

            return holder;
        }

        public override void OnViewRecycled(Java.Lang.Object holder)
        {
            var realHolder = holder as SearchRecyclerViewAdapterViewHolder;
            realHolder.ItemView.FindViewById(Resource.Id.thumbnail_image).Visibility = ViewStates.Invisible;
            realHolder.RecycleCount++;
        }

        public delegate void HolderClickedEventHandler(object sender, RecyclerView.ViewHolder holder);

        public event HolderClickedEventHandler HolderClick;

        public delegate void HolderLongClickEventHandler(object sender, RecyclerView.ViewHolder holder);

        public event HolderLongClickEventHandler HolderLongClick;

    }

    internal class SearchRecyclerViewAdapterViewHolder : Android.Support.V7.Widget.RecyclerView.ViewHolder
    {
        public int ParentWidth;
        public BooruPost Post;
        public int RecycleCount = 0;

        public SearchRecyclerViewAdapterViewHolder(View view) : base(view)
        {

        }
    }
}