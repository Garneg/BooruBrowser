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
using System.Threading.Tasks;

namespace BooruBrowser
{
    [XmlRoot(ElementName = "posts")]
    public class GelbooruSearchResult
    {
        [XmlElement(ElementName = "post")]
        public List<GelbooruPost> Posts;

        [XmlAttribute(AttributeName = "limit")]
        public int Limit;

        [XmlAttribute(AttributeName = "offset")]
        public int Offset;

        [XmlAttribute(AttributeName = "count")]
        public int TotalPostsCount;
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
        public async static Task<BooruSearchResult> SearchPosts(string[] tags, int pageNumber, int limit = 10)
        {
            WebClient client = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, chainName) => { return true; };
            string response = await client.DownloadStringTaskAsync($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit={limit}&pid={pageNumber}&tags={string.Join('+', tags)}");
            var doc = new XmlDocument();
            
            doc.LoadXml(response);
            XmlSerializer serializer = new XmlSerializer(typeof(GelbooruSearchResult));
            TextReader reader = new StringReader(doc.ChildNodes[1].OuterXml);
            var result = (serializer.Deserialize(reader) as GelbooruSearchResult);
            var resultPosts = result.Posts;
            List<BooruPost> boorus = new List<BooruPost>();

            foreach(var post in resultPosts)
            {
                boorus.Add(new BooruPost()
                {
                    PostId = post.Id,
                    PreviewUrl = post.PreviewUrl,
                    SampleUrl = post.SampleCount == 1 ? post.SampleUrl : post.FileUrl,
                    FileUrl = post.FileUrl,
                    Tags = post.Tags.Split(' '),
                    Score = post.Score,
                    Rating = post.Rating,
                    Source = post.Source,
                    Width = post.Width,
                    Height = post.Height,
                    PreviewWidth = post.PreviewWidth,
                    PreviewHeight = post.PreviewHeight,
                    SampleWidth = post.SampleCount == 1 ? post.SampleWidth : post.Width,
                    SampleHeight = post.SampleCount == 1 ? post.SampleHeight : post.Height
                });
            }

            BooruSearchResult booruSearchResult = new BooruSearchResult()
            {
                Posts = boorus,
                TotalPostsCount = result.TotalPostsCount,
                Offset = result.Offset
            };

            return booruSearchResult;

        }
       
        public static BooruAutocompleteItem[] Autocomplete(string part)
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;
            WebClient webClient = new WebClient();
            string response = WebUtility.HtmlDecode(webClient.DownloadString($"https://gelbooru.com//index.php?page=autocomplete2&term={part}"));
            var doc = JsonDocument.Parse(response);
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