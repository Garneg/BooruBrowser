using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BooruBrowser
{
    public class BooruSearchResult
    {
        public List<BooruPost> Posts;

        public int TotalPostsCount;

        public int Offset;

    }
    ///Remeber to encapsulate file sample and preview fields!!
    public class BooruPost
    {
        public string FileUrl { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string SampleUrl { get; set; }

        public int SampleWidth { get; set; }

        public int SampleHeight { get; set; }

        public string PreviewUrl { get; set; }

        public int PreviewWidth { get; set; }

        public int PreviewHeight { get; set; }


        public int Score { get; set; }

        public string ParentId { get; set; }

        public bool HasChildren { get; set; }

        public string Source { get; set; }

        public bool HasComments { get; set; }

        public bool HasNotes { get; set; }

        public string Status { get; set; }

        public long CreatorId { get; set; }

        public string Rating { get; set; }

        public string[] Tags { get; set; }

        public long PostId { get; set; }

        public string CreatedAt { get; set; }

        public long Change { get; set; }

    }

    /// <summary>
    /// Represents content information structure, contains its size and url
    /// </summary>
    public struct ContentUnit
    {
        public string Url { get; }
        public int Width { get; }
        public int Height { get; }

        public ContentUnit(string url, int width, int height)
        {
            Url = url;
            Width = width;
            Height = height;
        }
    }
}