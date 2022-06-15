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

namespace Rule34
{
    [Serializable, XmlRoot(ElementName = "posts")]
    public class PostsCollection
    {
        [XmlElement("post")]
        public List<BooruPost> posts;

        [XmlAttribute("count")]
        public int Count;

        public static PostsCollection FromXml(XmlDocument xmlDocument)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PostsCollection));
            TextReader reader = new StringReader(xmlDocument.ChildNodes[1].OuterXml);
            return (PostsCollection)serializer.Deserialize(reader);
        }


    }
    ///Remeber to encapsulate file sample and preview fields!!
    [Serializable, XmlRoot(ElementName = "post")]
    public class BooruPost
    {
        [XmlAttribute(AttributeName = "file_url")]
        public string fileUrl { get; set; }

        [XmlAttribute(AttributeName = "width")]
        public int width { get; set; }

        [XmlAttribute(AttributeName = "height")]
        public int height { get; set; }

        [XmlAttribute(AttributeName = "sample_url")]
        public string sampleUrl { get; set; }

        [XmlAttribute(AttributeName = "sample_width")]
        public int sampleWidth { get; set; }

        [XmlAttribute(AttributeName = "sample_height")]
        public int sampleHeight { get; set; }

        [XmlAttribute(AttributeName = "preview_url")]
        public string previewUrl { get; set; }

        [XmlAttribute(AttributeName = "preview_width")]
        public int previewWidth { get; set; }

        [XmlAttribute(AttributeName = "preview_height")]
        public int previewHeight { get; set; }



        [XmlAttribute(AttributeName = "score")]
        public int score { get; set; }

        [XmlAttribute(AttributeName = "parent_id")]
        public string parentId { get; set; }

        [XmlAttribute(AttributeName = "has_children")]
        public bool hasChildren { get; set; }

        [XmlAttribute(AttributeName = "source")]
        public string source { get; set; }

        [XmlAttribute(AttributeName = "has_comments")]
        public bool hasComments { get; set; }

        [XmlAttribute(AttributeName = "has_notes")]
        public bool hasNotes { get; set; }

        [XmlAttribute(AttributeName = "status")]
        public string status { get; set; }

        [XmlAttribute(AttributeName = "creator_id")]
        public long creatorId { get; set; }

        [XmlAttribute(AttributeName = "rating")]
        public string Rating { get; set; }

        [XmlAttribute(AttributeName = "tags")]
        public string[] Tags { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public long postId { get; set; }

        [XmlAttribute(AttributeName = "created_at")]
        public string createdAt { get; set; }

        [XmlAttribute(AttributeName = "change")]
        public long change { get; set; }

        public ContentUnit Sample
        {
            get => new ContentUnit(sampleUrl, sampleWidth, sampleHeight);
        }

        public ContentUnit Preview
        {
            get => new ContentUnit(previewUrl, previewWidth, previewHeight);
        }

        public ContentUnit File
        {
            get => new ContentUnit(fileUrl, width, height);

        }

        

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