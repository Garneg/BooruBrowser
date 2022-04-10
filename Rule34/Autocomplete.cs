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
        [JsonPropertyName("label")]
        public string Label { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }

        public int PostsCount { get; set; }

        public static List<Autocomplete> FromJson(JsonDocument document)
        {
            var autocompleteList = JsonSerializer.Deserialize<List<Autocomplete>>(document);
            string autocompleteLabel;
            for (int i = 0; i < autocompleteList.Count; i++)
            {
                autocompleteLabel = autocompleteList[i].Label;
                autocompleteList[i].PostsCount = int.Parse(
                    autocompleteLabel.Substring(autocompleteLabel.IndexOf('('), autocompleteLabel.IndexOf(')') - autocompleteLabel.IndexOf(')')));
            }
            return autocompleteList;
        }
    }
}