using Android.App;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using Android.Widget;
using Android.Views;
using System;
using System.Net;
using System.IO;
using Android.Graphics;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Xml;


namespace Rule34
{

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {

        private AutoCompleteTextView text;
        private TextView Output;
        private LinearLayout Container;
        private RelativeLayout relativeLayout;
        private int pageResultLimit = 10;
        private int pageNumber = 1;

        private ListView AutocompleteList;
        private Button NextPageButton;
        private Button PreviousPageButton;
        private LinearLayout Paginator;
        private TextView PageNumberIndicator;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Button searchBtn = FindViewById<Button>(Resource.Id.button1);
            Output = FindViewById<TextView>(Resource.Id.textView1);
            Output.Visibility = ViewStates.Gone;
            text = FindViewById<AutoCompleteTextView>(Resource.Id.autoCompleteTextView1);
            NextPageButton = FindViewById<Button>(Resource.Id.nextPageButton);
            PreviousPageButton = FindViewById<Button>(Resource.Id.previousPageButton);
            Paginator = FindViewById<LinearLayout>(Resource.Id.paginator);
            PageNumberIndicator = FindViewById<TextView>(Resource.Id.pageNumberIndicator);

            searchBtn.Click += Search;
            text.AfterTextChanged += Text_AfterTextChanged;
            FindViewById<ImageView>(Resource.Id.MikuTopImage).SetImageResource(Resource.Drawable.topb);
            relativeLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            //var list = FindViewById<ListView>(Resource.Id.contentList);
            Container = FindViewById<LinearLayout>(Resource.Id.container);
            AutocompleteList = new ListView(this);
            AutocompleteList.SetBackgroundColor(Color.White);
            AutocompleteList.Visibility = ViewStates.Invisible;
            AutocompleteList.SetPadding(10, 0, 10, 0);
            AutocompleteList.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, 450);
            AutocompleteList.ItemClick += AutocompleteList_ItemClick;
            NextPageButton.Click += NextPageButton_Click;
            PreviousPageButton.Click += PreviousPageButton_Click;
            PageNumberIndicator.SetMinimumWidth(PreviousPageButton.MinimumWidth);


            relativeLayout.AddView(AutocompleteList);
            Paginator.Visibility = ViewStates.Gone;
        }

        private void PreviousPageButton_Click(object sender, EventArgs e)
        {
            ///Probably needs to be redone, but it seems to be very safe, if we will forgot to disable it
            if (pageNumber > 1)
            {
                pageNumber--;
                if (pageNumber == 1)
                    PreviousPageButton.Enabled = false;
                Search(sender, e);

            }
        }

        private void NextPageButton_Click(object sender, EventArgs e)
        {
            pageNumber++;
            Search(sender, e);
            PreviousPageButton.Enabled = true;
        }

        private void AutocompleteList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string[] queryWords = text.Text.Split(' ');
            string autocompletedText = string.Empty;
            for (int i = 0; i < queryWords.Length - 1; i++)
            {
                autocompletedText += queryWords[i] + ' ';
            }
            autocompletedText += AutocompleteList.Adapter.GetItem(e.Position).ToString();
            text.Text = autocompletedText + ' ';
            text.SetSelection(text.Text.Length);
            AutocompleteList.Visibility = ViewStates.Invisible;
        }

        private async void Text_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            try
            {

                string query = text.Text.Split(' ')[text.Text.Split(' ').Length - 1];

                HttpWebRequest request = WebRequest.CreateHttp($"https://rule34.xxx/autocomplete.php?q={query}");

                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

                StreamReader reader = new StreamReader(response.GetResponseStream());
                string responseText = reader.ReadToEnd();
                if (AutocompleteList.Visibility == ViewStates.Invisible)
                    AutocompleteList.Visibility = ViewStates.Visible;
                AutocompleteList.SetY(text.TranslationY + text.Height + FindViewById<LinearLayout>(Resource.Id.linearLayout2).GetY());
                AutocompleteList.SetPadding(15, 0, 15, 0);
                string[] prompts = new string[responseText.Split("value\":\"").Length];
                for (int i = 0; i < prompts.Length; i++)
                {
                    responseText = responseText.Substring(responseText.IndexOf("value\":\"") + "value\":\"".Length);
                    prompts[i] = responseText.Substring(0, responseText.IndexOf('\"'));
                }
                AutocompleteList.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, prompts);
                ArrayAdapter<string> arrayAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, prompts);

            }
            catch (Exception ex)
            {
                //Output.Text = "oh shit " + ex.Message + "|||" + ex.StackTrace;
            }
        }

        public override void OnBackPressed()
        {
            //base.OnBackPressed();

            if (AutocompleteList.Visibility == ViewStates.Visible)
                AutocompleteList.Visibility = ViewStates.Invisible;

        }

        
        public async void Search(object sender, EventArgs e)
        {
            try
            {
                Paginator.Visibility = ViewStates.Gone;
                AutocompleteList.Visibility = ViewStates.Invisible;
                Output.Text = "";
                if (Container.ChildCount > 0)
                    Container.RemoveAllViews();
                string query = text.Text.Replace(" ", "+");
                HttpWebRequest request = WebRequest.CreateHttp($"https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&limit={pageResultLimit}&tags={query}&pid={(pageNumber - 1) * pageResultLimit}");

                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

                StreamReader responseReader = new StreamReader(response.GetResponseStream());

                string responseString = responseReader.ReadToEnd();

                if (responseString.IndexOf("sample_url=\"") < 0)
                {
                    Toast.MakeText(this, "Not found any image🤷‍♂️", ToastLength.Short).Show();
                    return;
                }

                await Xamarin.Essentials.Clipboard.SetTextAsync(responseString);
                string[] picturesUrls = new string[responseString.Split("sample_url=").Length - 1];
                Toast.MakeText(this, picturesUrls.Length.ToString(), ToastLength.Short).Show();

                for (int i = 0; i < picturesUrls.Length; i++)
                {
                    await Task.Run(() =>
                    {
                        string url = responseString.Substring(responseString.IndexOf("sample_url=\"") + "sample_url=\"".Length);
                        url = url.Substring(0, url.IndexOf("\""));

                        if (responseString.IndexOf("sample_url=\"") > 0)
                            responseString = responseString.Substring(responseString.IndexOf("sample_url=\"") + "sample_url =\"".Length);

                        //string url = responseString.Substring(responseString.IndexOf("preview_url=\"") + "preview_url=\"".Length);
                        //url = url.Substring(0, url.IndexOf("\""));

                        //if (responseString.IndexOf("preview_url=\"") > 0)
                        //    responseString = responseString.Substring(responseString.IndexOf("preview_url=\"") + "preview_url=\"".Length);


                        if (url.IndexOf(".mp4") > 0)
                        {
                            url = responseString.Substring(responseString.IndexOf("preview_url=\"") + "preview_url=\"".Length);

                            url = url.Substring(0, url.IndexOf("\""));

                        }
                        picturesUrls[i] = url;
                    });


                }
                int LoadedImagesNumber = 0;
                ImageView[] imageViews = new ImageView[picturesUrls.Length];
                for (int i = 0; i < picturesUrls.Length; i++)
                {
                    WebClient client = new WebClient();
                    string pictureUrl = picturesUrls[i];
                    byte[] bytesForImage = client.DownloadData(picturesUrls[i]);

                    Bitmap Picture = BitmapFactory.DecodeByteArray(bytesForImage, 0, bytesForImage.Length);
                    ImageView image = new ImageView(this);

                    if (Picture != null)
                    {

                        image.SetPadding(0, 10, 0, 10);

                        image.SetImageBitmap(Picture);

                        int height = (int)((float)Picture.Height / (float)Picture.Width * (float)Container.Width);

                        image.LayoutParameters = new ViewGroup.LayoutParams(Container.Width, height);

                        image.SetScaleType(ImageView.ScaleType.FitXy);

                        image.Click += (object sender, EventArgs e) =>
                        {
                            Toast.MakeText(this, $"Picture: {Picture.Width}x{Picture.Height}\n" +
                                $"ImageView: {image.Width}x{image.Height}\n" +
                                $"Container width: {Container.Width}\n" +
                                $"Height: {Picture.Height} / {Picture.Width} * {Container.Width} = {height}", ToastLength.Short).Show();
                        };
                        image.LongClick += (object sender, View.LongClickEventArgs e) =>
                        {
                            Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(this);
                            builder.SetTitle("Do you want to download this?");
                            builder.SetPositiveButton("Yes", (object sender, Android.Content.DialogClickEventArgs e) =>
                            {
                                DownloadManager manager = DownloadManager.FromContext(this);
                                DownloadManager.Request downloadRequest = new DownloadManager.Request(Android.Net.Uri.Parse(pictureUrl));

                                downloadRequest.SetNotificationVisibility(DownloadVisibility.VisibleNotifyCompleted);
                                downloadRequest.SetTitle(text.Text);
                                downloadRequest.SetDestinationInExternalPublicDir(Android.OS.Environment.DirectoryDownloads, $"{text.Text}.jpg");
                                long id = manager.Enqueue(downloadRequest);
                            });
                            Android.App.AlertDialog dialog = builder.Create();
                            dialog.Show();
                        };
                    }
                    else
                    {
                        image.SetMinimumWidth(500);
                        image.SetMinimumHeight(500);
                        image.SetBackgroundColor(TitleColor);
                    }

                    imageViews[i] = image;

                    LoadedImagesNumber++;


                }

                for (int i = 0; i < picturesUrls.Length; i++)
                {
                    while (LoadedImagesNumber < picturesUrls.Length)
                    {
                        Thread.Sleep(50);
                    }

                    Container.AddView(imageViews[i]);
                }

                PageNumberIndicator.Text = pageNumber.ToString();


                Paginator.Visibility = ViewStates.Visible;
            }
            catch (Exception ex)
            {
                Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(this);
                builder.SetTitle("Oops");
                builder.SetMessage("Something went wrong. If this is not the first time you get error try to search by other tags");
                builder.SetPositiveButton("Ok", (sender, eventargs) => { });
                builder.SetNeutralButton("Copy error message", (sender, eventargs) => { Xamarin.Essentials.Clipboard.SetTextAsync("Message: " + ex.Message + "\nStacktrace: " + ex.StackTrace); });
                builder.Create().Show();
            }
        }

        public async Task<Post[]> GetPostsURLs(string RequestURL)
        {

            HttpWebRequest request = WebRequest.CreateHttp(RequestURL);

            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

            return new Post[10];
            
        }
    }

    public class Post
    {
        string fileUrl;
        int width;
        int height;

        string sampleUrl;
        int sampleWidth;
        int sampleHeight;

        string previewUrl;
        int previewWidth;
        int previewHeight;

        int score;

        int? parentId;
        bool hasChildren;

        string source;

        bool hasComments;
        bool hasNotes;
        string status;

        long creatorId;

        string rating;

        string[] tags;

        long postId;

        string createdAt;

        long change;


    }

    public static class ExtensionClass
    {
        //public static char firstchar(this post post)
        //{
        //    return ;
        //}
    }
}