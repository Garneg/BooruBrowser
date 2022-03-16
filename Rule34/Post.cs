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
        public List<Post> posts;

        public static PostsCollection FromXml(string xmlDocument)
        {
            XmlDocument Document = new XmlDocument();
            Document.LoadXml(xmlDocument);
            XmlSerializer serializer = new XmlSerializer(typeof(PostsCollection));
            TextReader reader = new StringReader(Document.ChildNodes[1].OuterXml);
            return (PostsCollection)serializer.Deserialize(reader);
        }

    }

    [Serializable, XmlRoot(ElementName = "post")]
    public class Post
    {
        [XmlAttribute(AttributeName = "file_url")]
        public string FileUrl { get; }

        [XmlAttribute(AttributeName = "width")]
        public int Width { get; }

        [XmlAttribute(AttributeName = "height")]
        public int Height { get; }

        [XmlAttribute(AttributeName = "sample_url")]
        public string SampleUrl { get; }

        [XmlAttribute(AttributeName = "sample_width")]
        public int SampleWidth { get; }

        [XmlAttribute(AttributeName = "sample_height")]
        public int SampleHeight { get; }

        [XmlAttribute(AttributeName = "preview_url")]
        public string PreviewUrl { get; }

        [XmlAttribute(AttributeName = "preview_width")]
        int preview_width;

        [XmlAttribute(AttributeName = "preview_height")]
        int preview_height;

        [XmlAttribute(AttributeName = "score")]
        public int Score { get; }

        [XmlAttribute(AttributeName = "parent_id")]
        public string ParentId { get; }

        [XmlAttribute(AttributeName = "has_children")]
        bool has_children;

        [XmlAttribute(AttributeName = "source")]
        string source;

        [XmlAttribute(AttributeName = "has_comments")]
        bool has_comments;

        [XmlAttribute(AttributeName = "has_notes")]
        bool has_notes;

        [XmlAttribute(AttributeName = "status")]
        string status;

        [XmlAttribute(AttributeName = "creator_id")]
        public long CreatorId { get; }

        [XmlAttribute(AttributeName = "rating")]
        public string Rating { get; }

        [XmlAttribute(AttributeName = "tags")]
        public string[] tags { get; }

        [XmlAttribute(AttributeName = "post_id")]
        public long PostId { get; }

        [XmlAttribute(AttributeName = "created_at")]
        string created_at;

        [XmlAttribute(AttributeName = "change")]
        long change;


    }
}