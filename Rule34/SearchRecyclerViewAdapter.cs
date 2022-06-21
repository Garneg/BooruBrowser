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
            
            int height = (int)((float)posts[position].SampleHeight / posts[position].SampleWidth * (float)realHolder.ParentWidth / 2);

            thumb.Visibility = ViewStates.Invisible;
            thumb.SetMinimumHeight(height);
            text.Text = string.Join(' ', posts[position].Tags);
            text.SetMaxLines(2);
            
            if (cachedPreviews[position] != null)
            {
                thumb.SetImageBitmap(cachedPreviews[position]);
                thumb.Visibility = ViewStates.Visible;
            }
            

            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;

            WebClient webClient = new WebClient();
            string url = posts[position].ContentType == ContentType.Static ? posts[position].SampleUrl : posts[position].PreviewUrl;

            if (cachedPreviews[position] == null)
            {
                try
                {
                    byte[] previewBytes = await webClient.DownloadDataTaskAsync(posts[position].PreviewUrl);
                    Bitmap previewBitmap = await BitmapFactory.DecodeByteArrayAsync(previewBytes, 0, previewBytes.Length);
                    cachedPreviews[position] = Bitmap.CreateScaledBitmap(previewBitmap, holderView.Width, height, true);
                    if (holderRecycled == realHolder.RecycleCount)
                    {
                        thumb.SetImageBitmap(cachedPreviews[position]);
                        thumb.Visibility = ViewStates.Visible;
                    }
                }
                catch(Exception e)
                {

                }
            }
            try
            {
                byte[] imageBytes = await webClient.DownloadDataTaskAsync(url);
                Bitmap thumbBitmap = await BitmapFactory.DecodeByteArrayAsync(imageBytes, 0, imageBytes.Length);
                thumbBitmap = Bitmap.CreateScaledBitmap(thumbBitmap, realHolder.ItemView.Height, height, true);
                if (holderRecycled == realHolder.RecycleCount)
                {
                    thumb.SetImageBitmap(thumbBitmap);
                    thumb.Visibility = ViewStates.Visible;
                    //thumb.SetMaxHeight(height);
                    //thumb.SetMinimumHeight(height);
                }
            }
            catch(Exception e)
            {

            }
           
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.post_recyclerview_item, parent, false);

            var textview = view.FindViewById<TextView>(Resource.Id.post_recyclerview_item_additonal_text);
            textview.Click += (s, e) =>
            {
                if (textview.MaxLines == 2)
                    textview.SetMaxLines(1000);
                else
                    textview.SetMaxLines(2);
            };

            var holder = new SearchRecyclerViewAdapterViewHolder(view)
            {
                ParentWidth = parent.Width
            };

            view.Click += (s, e) => HolderClick.Invoke(this, holder);

            view.LongClick += (s, e) => HolderLongClick.Invoke(this, holder);

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