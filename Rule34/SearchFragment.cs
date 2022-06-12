using Android.App;
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

        private Button NextPageButton;
        private Button PreviousPageButton;
        private LinearLayout Paginator;
        private TextView PageNumberIndicator;
        private ProgressBar progressBar1;
        private Spinner sortBySpinner;
        private Spinner sortOrderSpinner;

        private int lastRequestHashCode = 0;
        private string searchSortBy = "updated";
        private string sortOrder = "desc";

        private ListPopupWindow autocompleteListWindow;

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

            searchBtn.Click += (s, e) => SearchNew();
            text.EditorAction += (s, e) => { if (e.ActionId == ImeAction.Search) SearchNew(); };


            text.AfterTextChanged += Text_AfterTextChanged;
            Activity.FindViewById<ImageView>(Resource.Id.MikuTopImage).SetImageResource(Resource.Drawable.topb);
            relativeLayout = Activity.FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            Container = Activity.FindViewById<LinearLayout>(Resource.Id.container);
            NextPageButton.Click += NextPageButton_Click;
            PreviousPageButton.Click += PreviousPageButton_Click;
            PageNumberIndicator.SetMinimumWidth(PreviousPageButton.MinimumWidth);

            Paginator.Visibility = ViewStates.Gone;


            var scrll = Activity.FindViewById<ScrollView>(Resource.Id.scrollView1);

            string[] spinnerAdapterArray = new string[] { "Default", "Updated", "Score", "Id" };

            sortBySpinner = Activity.FindViewById<Spinner>(Resource.Id.sortby_spinner);
            sortBySpinner.Adapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleListItem1, spinnerAdapterArray);
            sortBySpinner.ItemSelected += SortBy_Spinner_ItemSelected;

            string[] orderSpinnerAdapterArray = new string[] { "Descending", "Ascending" };

            sortOrderSpinner = Activity.FindViewById<Spinner>(Resource.Id.order_spinner);
            sortOrderSpinner.Adapter = new ArrayAdapter<string>(Context, Android.Resource.Layout.SimpleListItem1, orderSpinnerAdapterArray);
            sortOrderSpinner.ItemSelected += OrderSpinner_ItemSelected;

            (sortOrderSpinner.Parent as LinearLayout).Visibility = ViewStates.Gone;

            autocompleteListWindow = new ListPopupWindow(Activity);

            autocompleteListWindow.AnchorView = Activity.FindViewById(Resource.Id.SearchBox);

            autocompleteListWindow.Height = (int)(150 * Resources.DisplayMetrics.Density);

            autocompleteListWindow.ItemClick += AutocompleteList_ItemClick;
        }

       

        private string GetSortQuery()
        {
            if (searchSortBy == string.Empty || searchSortBy == null)
            {
                return string.Empty;
            }

            return $"sort:{searchSortBy}:{sortOrder}";
        }

        private void OrderSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            switch (e.Position)
            {
                case 0:
                    sortOrder = "desc";
                    break;
                case 1:
                    sortOrder = "asc";
                    break;
            }
        }

        private void SortBy_Spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            switch (e.Position)
            {
                case 0:
                    searchSortBy = string.Empty;
                    Activity.FindViewById<LinearLayout>(Resource.Id.order_spinner_container).Visibility = ViewStates.Gone;
                    return;
                case 1:
                    searchSortBy = "updated";
                    break;
                case 2:
                    searchSortBy = "score";
                    break;
                case 3:
                    searchSortBy = "id";
                    break;

            }

            (sortOrderSpinner.Parent as LinearLayout).Visibility = ViewStates.Visible;

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.search_fragment, container, false);
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

        }

        public override void OnViewStateRestored(Bundle savedInstanceState)
        {
            base.OnViewStateRestored(savedInstanceState);
        }

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);

        }


        private async void Text_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            if (e.ActionId == ImeAction.Search)
            {
                SearchNew();
            }
        }

        public async void SearchButtonClicked(object sender, EventArgs e)
        {
            SearchNew();
        }

        private async void SearchNew()
        {
            if ($"{GetSortQuery()}+{text.Text}" == lastQuery)
                return;
            autocompleteListWindow.Dismiss();
            lastQuery = $"{GetSortQuery()}+{text.Text}";
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
            var windowSender = sender as ListPopupWindow;
            List<string> tags = text.Text.Split(' ').SkipLast(1).ToList();

            tags.Add((windowSender.ListView.Adapter as AutocompleteListAdapter).GetItem(e.Position).Tag);
            text.Text = string.Join(' ', tags.ToArray()) + ' ';
            text.SetSelection(text.Text.Length);

            windowSender.Dismiss();
        }

        private async void Text_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            try
            {
                await Task.Run(() =>
                {

                    string firstText = text.Text;
                    Task.Delay(500).Wait(); // Preventing spam requests
                    if (text.Text != firstText)
                        return;

                    string query = text.Text.Split(' ').Last();

                    if (query == string.Empty)
                    {
                        return;
                    }
                    WebClient client = new WebClient();

                    client.Encoding = Encoding.UTF8;

                    string responseText = client.DownloadString($"https://rule34.xxx/autocomplete.php?q={query}");
                    responseText = WebUtility.HtmlDecode(responseText);

                    var autocompletes = Autocomplete.FromJson(JsonDocument.Parse(responseText));
                    if (autocompletes.Count > 0)
                    {
                        new Handler(Activity.MainLooper).Post(() =>
                        {
                            autocompleteListWindow.SetAdapter(new AutocompleteListAdapter(Activity, Resource.Layout.autocomplete_list_item, autocompletes.ToArray()));
                            autocompleteListWindow.Show();

                        });
                    }
                    else
                    {
                        new Handler(Activity.MainLooper).Post(() =>
                        {
                            autocompleteListWindow.Dismiss();
                        });
                    }
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
                if (Container.ChildCount > 0)
                    Container.RemoveAllViews();
                AppData.WriteLastQuery(lastQuery.Split(' ')[0]);
                string query = lastQuery.Replace(' ', '+');
                string requestURL = $"https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&limit={pageResultLimit}&tags={query}&pid={(pageNumber - 1)}";

                XmlDocument response = await RequestXml(requestURL);
                int currentRequestHashCode = requestURL.GetHashCode();

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
                            //var transaction = Activity.SupportFragmentManager.BeginTransaction();
                            //PostFragment fragment = new PostFragment(image.GetPost());
                            //transaction.SetReorderingAllowed(true);
                            //transaction.Replace(Resource.Id.main_frame_layout, fragment);
                            //transaction.Commit();
                        };
                        image.LongClick += (sender, e) =>
                        {
                            AndroidX.AppCompat.App.AlertDialog.Builder builder = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity);
                            builder.SetTitle("Download");
                            builder.SetMessage("Do you want to download the content of this post?");
                            builder.SetPositiveButton("Yes", (object sender, Android.Content.DialogClickEventArgs e) =>
                            {

                                DownloadManager manager = DownloadManager.FromContext(Activity);
                                DownloadManager.Request downloadRequest = new DownloadManager.Request(Android.Net.Uri.Parse(image.GetPost().File.Url));

                                downloadRequest.SetNotificationVisibility(DownloadVisibility.VisibleNotifyCompleted);
                                downloadRequest.SetTitle(image.GetPost().Tags[0]);
                                downloadRequest.SetDestinationInExternalPublicDir(Android.OS.Environment.DirectoryDownloads, $"{image.GetPost().Tags[0]}" +
                                    $"{image.GetPost().File.Url.Substring(image.GetPost().File.Url.LastIndexOf('.'))}");

                                new Handler(Activity.MainLooper).Post(() =>
                                {
                                    long id = manager.Enqueue(downloadRequest);

                                });
                            });
                            builder.SetNegativeButton("No", (object sender, Android.Content.DialogClickEventArgs e) => { });
                            AndroidX.AppCompat.App.AlertDialog dialog = builder.Create();
                            //dialog.Show();

                            string[] popupOptions = new string[] { "Download", "Vote Up" };
                            ListPopupWindow popupwindow = new ListPopupWindow(Activity);
                            popupwindow.SetAdapter(new ArrayAdapter<string>(Activity, Android.Resource.Layout.SimpleListItem1, popupOptions));
                            popupwindow.AnchorView = image;
                            popupwindow.Modal = false;
                            //popupwindow.InputMethodMode = ListPopupWindowInputMethodMode.NotNeeded;
                            popupwindow.ItemClick += (s, e) =>
                            {
                                var popup = s as ListPopupWindow;
                                var postThumb = popup.AnchorView as PostThumbnail;

                                switch (e.Position)
                                {
                                    case 0:
                                        dialog.Show();
                                        break;

                                    case 1:
                                        int votesUpdated = Rule34Api.VoteUp(postThumb.GetPost().postId);
                                        if (postThumb.GetPost().score != votesUpdated)
                                        {
                                            Toast.MakeText(Activity, $"Voted! The score of this post now: {votesUpdated}", ToastLength.Short).Show();
                                            postThumb.GetPost().score = votesUpdated;
                                        }
                                        break;

                                }
                                popup.Dismiss();


                            };
                            popupwindow.Show();
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
                        if (Sample.Width > 32766 && Sample.Width > Sample.Height)
                        {
                            float cropFactor = 32766f / Sample.Width;

                            int cropedWidth = 32766;
                            int cropedHeight = (int)((float)Sample.Height * cropFactor);

                            Sample = Bitmap.CreateScaledBitmap(Sample, cropedWidth, cropedHeight, true);
                        }
                        else if (Sample.Height > 32766 && Sample.Height > Sample.Width)
                        {
                            float cropFactor = 32766f / Sample.Height;

                            int cropedWidth = (int)((float)Sample.Width * cropFactor);
                            int cropedHeight = 32766;

                            Sample = Bitmap.CreateScaledBitmap(Sample, cropedWidth, cropedHeight, true);
                        }
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

      
    }


}
