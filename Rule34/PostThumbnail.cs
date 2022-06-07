using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Android.Graphics;


namespace Rule34
{
    public class PostThumbnail : ImageView
    {
        private Post post;
        public Post GetPost()
        {
            return post;
        }

        public void SetPost(Post post)
        {
            this.post = post;
            
        }

        public PostThumbnail(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public PostThumbnail(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        public PostThumbnail(Context context) : base(context)
        {
            Initialize();
        }

        private void Initialize()
        {
            
        }
    }
}