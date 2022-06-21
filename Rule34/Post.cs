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
    
    /// <summary>
    /// Base class for all booru's api post classes
    /// </summary>
    public class BooruPost
    {
        /// <summary>
        /// The original file url
        /// </summary>
        public string FileUrl { get; set; }

        /// <summary>
        /// The width of original file
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of original file
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Url of sample. Sample has better quality of image than preview, but if the Content
        /// type of post is NonStatic, no link to sample will be provided
        /// </summary>
        public string SampleUrl { get; set; }

        /// <summary>
        /// The width of sample
        /// </summary>
        public int? SampleWidth { get; set; }

        /// <summary>
        /// The height of sample
        /// </summary>
        public int? SampleHeight { get; set; }

        /// <summary>
        /// Url of preview. Preview has lower weight, but worse quality of image than sample and file 
        /// </summary>
        public string PreviewUrl { get; set; }

        /// <summary>
        /// The width of preview
        /// </summary>
        public int PreviewWidth { get; set; }

        /// <summary>
        /// Height of preview
        /// </summary>
        public int PreviewHeight { get; set; }

        /// <summary>
        /// Score of the post
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Id of parent. Can be null
        /// </summary>
        public long ParentId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool HasChildren { get; set; }

        /// <summary>
        /// Url of source of post
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool HasComments { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool HasNotes { get; set; }

        /// <summary>
        /// Status of post
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Id of creater of this post
        /// </summary>
        public long CreatorId { get; set; }

        /// <summary>
        /// Rating of post
        /// </summary>
        public PostRating Rating { get; set; }

        /// <summary>
        /// Tags of post
        /// </summary>
        public string[] Tags { get; set; }

        /// <summary>
        /// Id of post
        /// </summary>
        public long PostId { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public string CreatedAt { get; set; }

        public long Change { get; set; }

        public ContentType ContentType { get; set; }

    }

    /// <summary>
    /// Content type
    /// </summary>
    public enum ContentType : byte
    {
        /// <summary>
        /// Normal pictures in png or jpeg formats.
        /// </summary>
        Static = 0,
        /// <summary>
        /// Videos or animated gif's
        /// </summary>
        NonStatic = 1
    }

    /// <summary>
    /// Rating of post.
    /// </summary>
    public enum PostRating : byte
    {
        Explicit = 0,
        Questionable = 1,
        Safe = 2

    }
}