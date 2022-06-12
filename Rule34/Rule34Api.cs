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
using System.Net;

namespace Rule34
{
    /// <summary>
    /// Static class with carefully taken api methods.
    /// </summary>
    static class Rule34Api
    {
        public static int VoteUp(long postId)
        {
            WebClient client = new WebClient();
            string response = client.DownloadString($"https://rule34.xxx/index.php?page=post&s=vote&id={postId}&type=up");
            int votesUpdated = 0;
            int.TryParse(response, out votesUpdated);

            return votesUpdated;
        }

        

    }
}