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

namespace Rule34
{
    [JsonSerializable(typeof(Autocomplete))]
    public class Autocomplete
    {
        [JsonPropertyName("value")]
        public string Tag { get; set; }

        [JsonPropertyName("label")]
        public string label { get; set; }

       

        public int PostsCount { get; set; }

        public static List<Autocomplete> FromJson(JsonDocument document)
        {
            var autocompleteList = JsonSerializer.Deserialize<List<Autocomplete>>(document);
            string autocompleteLabel;
            for (int i = 0; i < autocompleteList.Count; i++)
            {
                autocompleteLabel = autocompleteList[i].label;
                string postsCount = autocompleteLabel.Substring(autocompleteLabel.LastIndexOf('(') + 1, autocompleteLabel.Length - autocompleteLabel.LastIndexOf('(') - 2);
                autocompleteList[i].PostsCount = int.Parse(postsCount);
            }
            return autocompleteList;
        }

    }

    public class AutocompleteListAdapter : ArrayAdapter
    {
        public Autocomplete[] autocompletes;
        public AutocompleteListAdapter(Context context, int resource, Autocomplete[] objects) : base(context, resource, objects)
        {
            autocompletes = objects;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            Autocomplete prompt = autocompletes[position];
            if (convertView == null)
            {
                convertView = LayoutInflater.FromContext(Context).Inflate(Resource.Layout.autocomplete_list_item, parent, false);
            }
            var autocompleteTag = convertView.FindViewById<TextView>(Resource.Id.autocomplete_item_label);
            autocompleteTag.Text = prompt.Tag;
            var autocompletePostsCount = convertView.FindViewById<TextView>(Resource.Id.autocomplete_item_posts_count);
            autocompletePostsCount.Text = prompt.PostsCount.ToString();
            return convertView;
        }

        public new Autocomplete GetItem(int position)
        {
            return autocompletes[position];
        }


    }
}