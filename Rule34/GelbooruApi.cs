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
using System.Xml;
using System.Xml.Serialization;
using System.Net;
using System.IO;

namespace Rule34
{
    [XmlRoot(ElementName = "posts")]
    public class PostCollection
    {
        [XmlElement(ElementName = "post")]
        public List<GelbooruPost> posts;

    }
    [XmlRoot(ElementName = "post")]
    public class GelbooruPost
    {
        [XmlElement(ElementName = "preview_url")]
        public string PreviewUrl;

        [XmlElement(ElementName = "sample_url")]
        public string SampleUrl;
    }
    class GelbooruApi
    {
        public static BooruPost[] PostsList(string[] tags, int limit = 10)
        {
            WebClient client = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, chainName) => { return true; };
            string response = client.DownloadString($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&tags={string.Join('+', tags)}&limit={limit}");
            var doc = new XmlDocument();
            doc.LoadXml(response);
            XmlSerializer serializer = new XmlSerializer(typeof(PostCollection));
            TextReader reader = new StringReader(doc.ChildNodes[1].OuterXml);
            var result = (serializer.Deserialize(reader) as PostCollection).posts;

            List<BooruPost> boorus = new List<BooruPost>();

            foreach(var post in result)
            {
                boorus.Add(new BooruPost()
                {
                    previewUrl = post.PreviewUrl,
                    sampleUrl = post.SampleUrl ?? post.PreviewUrl,
                });
            }
            return boorus.ToArray();

        }
       


        

    }
}