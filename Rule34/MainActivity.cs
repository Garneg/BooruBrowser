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
using System.Xml.Serialization;


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
        private ProgressBar progressBar1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Window.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);
                        
            Button searchBtn = FindViewById<Button>(Resource.Id.button1);
            Output = FindViewById<TextView>(Resource.Id.textView1);
            Output.Visibility = ViewStates.Gone;
            text = FindViewById<AutoCompleteTextView>(Resource.Id.autoCompleteTextView1);
            NextPageButton = FindViewById<Button>(Resource.Id.nextPageButton);
            PreviousPageButton = FindViewById<Button>(Resource.Id.previousPageButton);
            Paginator = FindViewById<LinearLayout>(Resource.Id.paginator);
            PageNumberIndicator = FindViewById<TextView>(Resource.Id.pageNumberIndicator);
            progressBar1 = FindViewById<ProgressBar>(Resource.Id.progressBar1);

            searchBtn.Click += SearchButtonClicked;
            text.AfterTextChanged += Text_AfterTextChanged;
            FindViewById<ImageView>(Resource.Id.MikuTopImage).SetImageResource(Resource.Drawable.topb);
            relativeLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
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

        public void SearchButtonClicked(object sender, EventArgs e)
        {
            Paginator.Visibility = ViewStates.Gone;
            PreviousPageButton.Enabled = false;
            pageNumber = 1;
            Thread thread = new Thread(new ThreadStart(() => { }));
            
        }

        private void PreviousPageButton_Click(object sender, EventArgs e)
        {
            ///Probably needs to be redone, but seems to be safe, if we will forgot to disable it
            if (pageNumber > 1)
            {
                pageNumber--;
                if (pageNumber == 1)
                    PreviousPageButton.Enabled = false;
                Search();
            }
        }

        private void NextPageButton_Click(object sender, EventArgs e)
        {
            pageNumber++;
            Search();
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
                await Task.Run(() =>
                {
                    string query = text.Text.Split(' ')[text.Text.Split(' ').Length - 1];

                    HttpWebRequest request = WebRequest.CreateHttp($"https://rule34.xxx/autocomplete.php?q={query}");

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string responseText = reader.ReadToEnd();
                    
                    string[] prompts = new string[responseText.Split("value\":\"").Length];
                    for (int i = 0; i < prompts.Length; i++)
                    {
                        responseText = responseText.Substring(responseText.IndexOf("value\":\"") + "value\":\"".Length);
                        prompts[i] = responseText.Substring(0, responseText.IndexOf('\"'));
                    }
                    new Handler(MainLooper).Post(() =>
                    {
                        AutocompleteList.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, prompts);
                        AutocompleteList.SetY(text.TranslationY + text.Height + FindViewById<LinearLayout>(Resource.Id.linearLayout2).GetY());
                        if (AutocompleteList.Visibility == ViewStates.Invisible)
                            AutocompleteList.Visibility = ViewStates.Visible;
                    });
                });
                //ArrayAdapter<string> arrayAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, prompts);

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

        public async void Search()
        {
            try
            {
                Paginator.Visibility = ViewStates.Gone;
                AutocompleteList.Visibility = ViewStates.Invisible;
                Output.Text = "";
                if (Container.ChildCount > 0)
                    Container.RemoveAllViews();
                string query = text.Text.Replace(" ", "+");
                
                XmlDocument response = await RequestXml($"https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&limit={pageResultLimit}&tags={query}&pid={(pageNumber - 1) * pageResultLimit}");

                if (response.ChildNodes[1].ChildNodes.Count < 1)
                {
                    Toast.MakeText(this, "Not found any image🤷‍♂️", ToastLength.Short).Show();
                    return;
                }

                var Collection = PostsCollection.FromXml(response);
                
                for (int i = 0; i < Collection.posts.Count; i++)
                {
                    await Task.Run(() =>
                    {
                        WebClient client = new WebClient();
                        string pictureUrl = Collection[i].SampleUrl;
                        byte[] bytesForImage = client.DownloadData(pictureUrl);

                        Bitmap Picture = BitmapFactory.DecodeByteArray(bytesForImage, 0, bytesForImage.Length);
                        PostThumbnail image = new PostThumbnail(this);
                        
                        if (Picture != null)
                        {

                            image.SetPadding(0, 10, 0, 10);

                            image.SetImageBitmap(Picture);
                            
                            int height = (int)((float)Picture.Height / (float)Picture.Width * (float)Container.Width);

                            image.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, height);

                            image.SetScaleType(ImageView.ScaleType.FitXy);

                            image.Click += (object sender, EventArgs e) =>
                            {
                                Toast.MakeText(this, $"Well it is a picture and you are definitely not allowed to download it by long click!", ToastLength.Short).Show();
                            };
                            image.LongClick += (object sender, View.LongClickEventArgs e) =>
                            {
                                Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(this);
                                builder.SetTitle("Download");
                                builder.SetMessage("Do you really want to download this piece of image?");
                                builder.SetPositiveButton("Yep", (object sender, Android.Content.DialogClickEventArgs e) =>
                                {
                                    DownloadManager manager = DownloadManager.FromContext(this);
                                    DownloadManager.Request downloadRequest = new DownloadManager.Request(Android.Net.Uri.Parse(pictureUrl));

                                    downloadRequest.SetNotificationVisibility(DownloadVisibility.VisibleNotifyCompleted);
                                    downloadRequest.SetTitle(text.Text);
                                    downloadRequest.SetDestinationInExternalPublicDir(Android.OS.Environment.DirectoryDownloads, $"{text.Text}.jpg");
                                    long id = manager.Enqueue(downloadRequest);
                                });
                                builder.SetNegativeButton("No", (object sender, Android.Content.DialogClickEventArgs e) => { });
                                Android.App.AlertDialog dialog = builder.Create();
                                dialog.Show();
                            };
                        }
                        RegisterForContextMenu(image);
                        new Handler(MainLooper).Post(() =>
                        {
                            Container.AddView(image);
                        });

                    });
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

        public async Task<XmlDocument> RequestXml(string RequestUrl)
        {
            HttpWebRequest request = WebRequest.CreateHttp(RequestUrl);

            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            XmlDocument doc = new XmlDocument();

            doc.LoadXml(responseReader.ReadToEnd());

            return doc;
        }
    }
}