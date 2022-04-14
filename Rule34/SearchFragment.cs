﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndroidX;
using System.Xml;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using AndroidX.AppCompat.App;
using System.IO;
using Android.Graphics;
using System.Net.NetworkInformation;
using System.Xml.Serialization;
using System.Text.Json.Serialization;
using Android.InputMethodServices;
using Android.Views.InputMethods;

namespace Rule34
{
    public class SearchFragment : AndroidX.Fragment.App.Fragment
    {
        private AutoCompleteTextView text;
        private LinearLayout Container;
        private RelativeLayout relativeLayout;
        private int pageResultLimit = 10;
        private int pageNumber = 1;
        private string lastQuery;

        private ListView AutocompleteList;
        private Button NextPageButton;
        private Button PreviousPageButton;
        private LinearLayout Paginator;
        private TextView PageNumberIndicator;
        private ProgressBar progressBar1;

        private int lastRequestHashCode = 0;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            Button searchBtn = Activity.FindViewById<Button>(Resource.Id.button1);
            text = Activity.FindViewById<AutoCompleteTextView>(Resource.Id.autoCompleteTextView1);
            NextPageButton = Activity.FindViewById<Button>(Resource.Id.nextPageButton);
            PreviousPageButton = Activity.FindViewById<Button>(Resource.Id.previousPageButton);
            Paginator = Activity.FindViewById<LinearLayout>(Resource.Id.paginator);
            PageNumberIndicator = Activity.FindViewById<TextView>(Resource.Id.pageNumberIndicator);
            progressBar1 = Activity.FindViewById<ProgressBar>(Resource.Id.progressBar1);

            Paginator.SetPadding(Paginator.PaddingLeft, 0, Paginator.PaddingRight, Paginator.PaddingBottom);

            searchBtn.Click += SearchButtonClicked;
            
            text.AfterTextChanged += Text_AfterTextChanged;
            Activity.FindViewById<ImageView>(Resource.Id.MikuTopImage).SetImageResource(Resource.Drawable.topb);
            relativeLayout = Activity.FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            Container = Activity.FindViewById<LinearLayout>(Resource.Id.container);
            AutocompleteList = new ListView(Activity);
            AutocompleteList.SetBackgroundColor(Android.Graphics.Color.White);
            AutocompleteList.Visibility = ViewStates.Invisible;
            AutocompleteList.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, (int)(140 * Resources.DisplayMetrics.Density));
            AutocompleteList.ItemClick += AutocompleteList_ItemClick;
            NextPageButton.Click += NextPageButton_Click;
            PreviousPageButton.Click += PreviousPageButton_Click;
            PageNumberIndicator.SetMinimumWidth(PreviousPageButton.MinimumWidth);

            relativeLayout.AddView(AutocompleteList);
            Paginator.Visibility = ViewStates.Gone;

            text.EditorAction += Text_EditorAction;

            text.Hint = AppData.ReadLastQuery();

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.search_fragment, container, false);
        }


        private async void Text_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            if (e.ActionId == ImeAction.Search)
            {
                lastQuery = text.Text;
                Paginator.Visibility = ViewStates.Gone;
                PreviousPageButton.Enabled = false;
                pageNumber = 1;
                await Search();
            }
        }

        public async void SearchButtonClicked(object sender, EventArgs e)
        {
            if (text.Text == lastQuery)
                return;
            lastQuery = text.Text;
            Paginator.Visibility = ViewStates.Gone;
            PreviousPageButton.Enabled = false;
            pageNumber = 1;
            await Search();
        }

        private async void PreviousPageButton_Click(object sender, EventArgs e)
        {
            pageNumber--;
            await Search();
        }

        private async void NextPageButton_Click(object sender, EventArgs e)
        {
            pageNumber++;
            await Search();
        }

        private void AutocompleteList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string[] queryWords = text.Text.Split(' ');
            string autocompletedText = string.Empty;
            for (int i = 0; i < queryWords.Length - 1; i++)
            {
                autocompletedText += queryWords[i] + ' ';
            }
            autocompletedText += (AutocompleteList.Adapter as AutocompleteListAdapter).GetItem(e.Position).Value;
            text.Text = autocompletedText + ' ';
            text.SetSelection(text.Text.Length);
            AutocompleteList.Visibility = ViewStates.Invisible;
        }

        private async void Text_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            try
            {
                await Task.Run(() =>
                {
                    string firstText = text.Text;
                    Task.Delay(250).Wait();
                    if (text.Text != firstText)
                    {
                        return;
                    }
                    string query = text.Text.Split(' ')[text.Text.Split(' ').Length - 1];

                    WebClient client = new WebClient();
                    client.Encoding = System.Text.Encoding.UTF8;

                    string responseText = client.DownloadString($"https://rule34.xxx/autocomplete.php?q={query}");
                    responseText = WebUtility.HtmlDecode(responseText);

                    Autocomplete[] autocompletes = Autocomplete.FromJson(JsonDocument.Parse(responseText)).ToArray();

                    new Handler(Activity.MainLooper).Post(() =>
                    {
                        AutocompleteList.Adapter = new AutocompleteListAdapter(Activity, Resource.Layout.autocomplete_list_item, autocompletes);
                        AutocompleteList.SetY(text.TranslationY + text.Height + Activity.FindViewById<LinearLayout>(Resource.Id.SearchBox).GetY());
                        if (AutocompleteList.Visibility == ViewStates.Invisible)
                            AutocompleteList.Visibility = ViewStates.Visible;
                    });
                });
            }
            catch (Exception ex)
            {
#if DEBUG 
                Toast.MakeText(Activity, "Error occured: " + ex.Message, ToastLength.Short).Show();
#endif
            }
        }

        //public override void OnBackPressed()
        //{
        //    //base.OnBackPressed();

        //    if (AutocompleteList.Visibility == ViewStates.Visible)
        //        AutocompleteList.Visibility = ViewStates.Invisible;

        //}

        public async Task Search()
        {
            try
            {

#if DEBUG
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
#endif
                InputMethodManager imm = (InputMethodManager)Activity.GetSystemService(Android.Content.Context.InputMethodService);
                imm.HideSoftInputFromWindow(text.ApplicationWindowToken, HideSoftInputFlags.None);
                Paginator.Visibility = ViewStates.Gone;
                AutocompleteList.Visibility = ViewStates.Invisible;
                if (Container.ChildCount > 0)
                    Container.RemoveAllViews();
                AppData.WriteLastQuery(lastQuery.Split(' ')[0]);
                string query = lastQuery.Replace(" ", "+");

                XmlDocument response = await RequestXml($"https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&limit={pageResultLimit}&tags={query}&pid={(pageNumber - 1)}");
                int currentRequestHashCode = $"https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&limit={pageResultLimit}&tags={query}&pid={(pageNumber - 1)}".GetHashCode();

                if (response.ChildNodes[1].ChildNodes.Count < 1)
                {
                    Toast.MakeText(Activity, "Not found any image🤷‍♂️", ToastLength.Short).Show();
                    return;
                }

                var Collection = PostsCollection.FromXml(response);
                List<PostThumbnail> postThumbnails = new List<PostThumbnail>();

                for (int i = 0; i < Collection.posts.Count; i++)
                {
                    if (currentRequestHashCode != lastRequestHashCode)
                        return;

                    await Task.Run(() =>
                    {
                        WebClient client = new WebClient();

                        Post currentPost = Collection[i];
                        string pictureUrl;

                        byte[] previewBytes = client.DownloadData(currentPost.Preview.Url);

                        Bitmap Preview = BitmapFactory.DecodeByteArray(previewBytes, 0, previewBytes.Length);
                        PostThumbnail image = new PostThumbnail(Activity);

                        postThumbnails.Add(image);

                        image.SetPadding(0, 10, 0, 10);
                        image.SetImageBitmap(Preview);
                        image.SetPost(currentPost);

                        int height = (int)((float)Preview.Height / (float)Preview.Width * (float)Container.Width);

                        image.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, height);

                        image.SetScaleType(ImageView.ScaleType.FitXy);

                        image.Click += (object sender, EventArgs e) =>
                        {
                            Toast.MakeText(Activity, $"Tags of post: {string.Join(' ', image.GetPost().Tags)}", ToastLength.Short).Show();
                        };
                        image.LongClick += (sender, e) =>
                        {
                            AndroidX.AppCompat.App.AlertDialog.Builder builder = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity);
                            builder.SetTitle("Download");
                            builder.SetMessage("Do you really want to download this piece of image?");
                            builder.SetPositiveButton("Yep", (object sender, Android.Content.DialogClickEventArgs e) =>
                            {
                                DownloadManager manager = DownloadManager.FromContext(Activity);
                                DownloadManager.Request downloadRequest = new DownloadManager.Request(Android.Net.Uri.Parse(image.GetPost().File.Url));

                                downloadRequest.SetNotificationVisibility(DownloadVisibility.VisibleNotifyCompleted);
                                downloadRequest.SetTitle(image.GetPost().Tags[0]);
                                downloadRequest.SetDestinationInExternalPublicDir(Android.OS.Environment.DirectoryDownloads, $"{image.GetPost().Tags[0]}" +
                                    $"{image.GetPost().File.Url.Substring(image.GetPost().File.Url.LastIndexOf('.'))}");
                                long id = manager.Enqueue(downloadRequest);
                            });
                            builder.SetNegativeButton("No", (object sender, Android.Content.DialogClickEventArgs e) => { });
                            AndroidX.AppCompat.App.AlertDialog dialog = builder.Create();
                            dialog.Show();
                        };

                        if (currentRequestHashCode == lastRequestHashCode)
                        {
                            Task.Run(() =>
                            {
                                new Handler(Activity.MainLooper).Post(() =>
                                {
                                    Container.AddView(image);
                                });
                            });
                        }
                    });
                }
                UpdatePaginator();
                Paginator.Visibility = ViewStates.Visible;
                await Task.Run(() =>
                {
                    Parallel.For(0, Collection.posts.Count, i =>
                    {
                        WebClient client = new WebClient();

                        if (postThumbnails[i].GetPost().Sample.Url.Contains(".mp4"))
                            return;
                        byte[] sampleBytes = client.DownloadData(postThumbnails[i].GetPost().Sample.Url);

                        Bitmap Sample = BitmapFactory.DecodeByteArray(sampleBytes, 0, sampleBytes.Length);
                        Task.Run(() =>
                        {
                            new Handler(Activity.MainLooper).Post(() =>
                            {
                                postThumbnails[i].SetImageBitmap(Sample);
                            });
                        });
                    });
                });
#if DEBUG 
                stopwatch.Stop();
                Toast.MakeText(Activity, "Page load time: " + stopwatch.ElapsedMilliseconds.ToString(), ToastLength.Short).Show();
#endif
            }
            catch (Exception ex)
            {
                AndroidX.AppCompat.App.AlertDialog.Builder builder = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity);

                builder.SetTitle("Oops");
                builder.SetMessage("Something went wrong. If this is not the first time you get error try to search by other tags");
                builder.SetPositiveButton("Ok", (sender, eventargs) => { });
                builder.SetNeutralButton("Copy error message", (sender, eventargs) => { Xamarin.Essentials.Clipboard.SetTextAsync("Message: " + ex.Message + "\nStacktrace: " + ex.StackTrace); });
                builder.Create().Show();
            }
        }

        public async void UpdatePaginator()
        {
            string query = lastQuery.Replace(" ", "+");
            XmlDocument nextPageDocument = await RequestXml($"https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&limit={pageResultLimit}&tags={query}&pid={(pageNumber)}");
            if (pageNumber > 1)
                PreviousPageButton.Enabled = true;
            else
                PreviousPageButton.Enabled = false;

            if (nextPageDocument.ChildNodes[1].ChildNodes.Count > 0)
            {
                NextPageButton.Enabled = true;
            }
            else
            {
                NextPageButton.Enabled = false;
            }
            PageNumberIndicator.Text = pageNumber.ToString();
        }
        
        public async Task<XmlDocument> RequestXml(string RequestUrl)
        {
            lastRequestHashCode = RequestUrl.GetHashCode();
            HttpWebRequest request = WebRequest.CreateHttp(RequestUrl);

            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            XmlDocument doc = new XmlDocument();

            doc.LoadXml(await responseReader.ReadToEndAsync());

            return doc;
        }

        public void HideAutocompleteList()
        {
            if (AutocompleteList != null)
                AutocompleteList.Visibility = ViewStates.Invisible;
        }
    }
}