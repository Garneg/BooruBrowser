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

namespace BooruBrowser
{
    public class BooruAutocompleteItem
    {
        public string TagLabel { get; set; }

        public string TagValue { get; set; }

        public string TagCategory { get; set; }

        public int PostsCount { get; set; }
    }

    public class AutocompleteListAdapter : ArrayAdapter
    {
        public BooruAutocompleteItem[] autocompletes;
        public AutocompleteListAdapter(Context context, int resource, BooruAutocompleteItem[] objects) : base(context, resource, objects)
        {
            autocompletes = objects;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            BooruAutocompleteItem prompt = autocompletes[position];
            
            convertView ??= LayoutInflater.FromContext(Context).Inflate(Resource.Layout.autocomplete_list_item, parent, false);
            
            var autocompleteTag = convertView.FindViewById<TextView>(Resource.Id.autocomplete_item_label);
            autocompleteTag.Text = prompt.TagLabel ?? prompt.TagValue;
            var autocompletePostsCount = convertView.FindViewById<TextView>(Resource.Id.autocomplete_item_posts_count);
            autocompletePostsCount.Text = prompt.PostsCount.ToString();
            return convertView;
        }

        public new BooruAutocompleteItem GetItem(int position)
        {
            return autocompletes[position];
        }


    }
}