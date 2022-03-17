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

        public static PostsCollection FromXml(XmlDocument xmlDocument)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PostsCollection));
            TextReader reader = new StringReader(xmlDocument.ChildNodes[1].OuterXml);
            return (PostsCollection)serializer.Deserialize(reader);
        }

        public Post this[int index]
        {
            get
            {
                return posts[index];
            }
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
        public int PreviewWidth { get; }

        [XmlAttribute(AttributeName = "preview_height")]
        public int PreviewHeight { get; }

        [XmlAttribute(AttributeName = "score")]
        public int Score { get; }

        [XmlAttribute(AttributeName = "parent_id")]
        public string ParentId { get; }

        [XmlAttribute(AttributeName = "has_children")]
        public bool HasChildren { get; }

        [XmlAttribute(AttributeName = "source")]
        public string Source { get; }

        [XmlAttribute(AttributeName = "has_comments")]
        public bool HasComments { get; }

        [XmlAttribute(AttributeName = "has_notes")]
        public bool HasNotes { get; }

        [XmlAttribute(AttributeName = "status")]
        string status;

        [XmlAttribute(AttributeName = "creator_id")]
        public long CreatorId { get; }

        [XmlAttribute(AttributeName = "rating")]
        public string Rating { get; }

        [XmlAttribute(AttributeName = "tags")]
        public string[] Tags { get; }

        [XmlAttribute(AttributeName = "post_id")]
        public long PostId { get; }

        [XmlAttribute(AttributeName = "created_at")]
        public string CreatedAt { get; }

        [XmlAttribute(AttributeName = "change")]
        public long Change { get; }


    }
}