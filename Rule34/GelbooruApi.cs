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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.Net;
using System.IO;

namespace BooruBrowser
{
    [XmlRoot(ElementName = "posts")]
    public class SearchPage
    {
        [XmlElement(ElementName = "post")]
        public List<GelbooruPost> Posts;

        [XmlAttribute(AttributeName = "limit")]
        public int Limit;

        [XmlAttribute(AttributeName = "offset")]
        public int Offset;

        [XmlAttribute(AttributeName = "count")]
        public int Count;
    }

    [XmlRoot(ElementName = "post")]
    public class GelbooruPost
    {
        [XmlElement(ElementName = "id")]
        public int Id;

        [XmlElement(ElementName = "created_at")]
        public string CreatedAt;

        [XmlElement(ElementName = "score")]
        public int Score;

        [XmlElement(ElementName = "source")]
        public string Source;
        
        [XmlElement(ElementName = "rating")]
        public string Rating;

        [XmlElement(ElementName = "owner")]
        public string Owner;

        [XmlElement(ElementName = "creator_id")]
        public long CreatorId;

        [XmlElement(ElementName = "tags")]
        public string Tags;

        // File
        [XmlElement(ElementName = "file_url")]
        public string FileUrl;

        [XmlElement(ElementName = "width")]
        public int Width;

        [XmlElement(ElementName = "height")]
        public int Height;

        // Preview
        [XmlElement(ElementName = "preview_url")]
        public string PreviewUrl;
        
        [XmlElement(ElementName = "preview_width")]
        public int PreviewWidth;

        [XmlElement(ElementName = "preview_height")]
        public int PreviewHeight;

        // Sample
        [XmlElement(ElementName = "sample_url")]
        public string SampleUrl;

        [XmlElement(ElementName = "sample")]
        public int SampleCount;

        [XmlElement(ElementName = "sample_width")]
        public int SampleWidth;

        [XmlElement(ElementName = "sample_height")]
        public int SampleHeight;



    }

    [JsonSerializable(typeof(GelbooruAutocompleteItem))]
    public class GelbooruAutocompleteItem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("post_count")]
        public string PostCount { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }
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
            XmlSerializer serializer = new XmlSerializer(typeof(SearchPage));
            TextReader reader = new StringReader(doc.ChildNodes[1].OuterXml);
            var result = (serializer.Deserialize(reader) as SearchPage).Posts;

            List<BooruPost> boorus = new List<BooruPost>();

            foreach(var post in result)
            {
                boorus.Add(new BooruPost()
                {
                    previewUrl = post.PreviewUrl,
                    sampleUrl = post.SampleUrl ?? post.PreviewUrl,
                    fileUrl = post.FileUrl,
                    Tags = post.Tags.Split(' '),
                    score = post.Score,
                    Rating = post.Rating,
                    source = post.Source,
                    width = post.Width,
                    height = post.Height,
                    previewWidth = post.PreviewWidth,
                    previewHeight = post.PreviewHeight,
                    sampleWidth = post.SampleCount == 1 ? post.SampleWidth : post.PreviewWidth,
                    sampleHeight = post.SampleCount == 1 ? post.SampleHeight : post.PreviewHeight
                });
            }
            return boorus.ToArray();

        }
       
        public static BooruAutocompleteItem[] Autocomplete(string part)
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;
            WebClient webClient = new WebClient();
            string response = WebUtility.HtmlDecode(webClient.DownloadString($"https://gelbooru.com//index.php?page=autocomplete2&term={part}"));
            var doc = JsonDocument.Parse(response);
            var l = doc.RootElement.EnumerateArray().ToArray()[0];
            var autocompletes = JsonSerializer.Deserialize<List<GelbooruAutocompleteItem>>(doc);

            List<BooruAutocompleteItem> booruAutocompleteItems = new List<BooruAutocompleteItem>();

            foreach(var gbautocompleteitem in autocompletes)
            {
                booruAutocompleteItems.Add(new BooruAutocompleteItem()
                {
                    TagLabel = gbautocompleteitem.Label,
                    TagValue = gbautocompleteitem.Value,
                    PostsCount = int.Parse(gbautocompleteitem.PostCount),
                    TagCategory = gbautocompleteitem.Category
                });
            }
            
            return booruAutocompleteItems.ToArray();
        }

        

    }
}