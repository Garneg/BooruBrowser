using Android.App;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using Android.Widget;
using Android.Views;
//using AndroidX.AppCompat.Widget;
using System;
using System.Net;
using System.IO;
using Android.Graphics;
using System.Threading;
using System.Threading.Tasks;


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
        private int pageid = 1;

        private ListView AutocompleteList;
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            //this.RequestWindowFeature(WindowFeatures.NoTitle);


            Button searchBtn = FindViewById<Button>(Resource.Id.button1);
            Output = FindViewById<TextView>(Resource.Id.textView1);
            Output.Visibility = ViewStates.Gone;
            text = FindViewById<AutoCompleteTextView>(Resource.Id.autoCompleteTextView1);
            searchBtn.Click += Search;
            
            text.AfterTextChanged += Text_AfterTextChanged;
            FindViewById<ImageView>(Resource.Id.MikuTopImage).SetImageResource(Resource.Drawable.topb);
            //ArrayAdapter adapter = new ArrayAdapter<String>(this, Resource.Id.contentList, mobileArray);
            relativeLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            //var list = FindViewById<ListView>(Resource.Id.contentList);
            Container = FindViewById<LinearLayout>(Resource.Id.container);
            AutocompleteList = new ListView(this);
            AutocompleteList.SetBackgroundColor(Color.White);
            AutocompleteList.Visibility = ViewStates.Invisible;
            AutocompleteList.SetPadding(10, 0, 10, 0);
            AutocompleteList.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, 450);
            AutocompleteList.ItemClick += AutocompleteList_ItemClick;
            
            relativeLayout.AddView(AutocompleteList);
        }

        private void AutocompleteList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string[] queryWords = text.Text.Split(' ');
            string autocompletedText = string.Empty;
            for(int i = 0; i < queryWords.Length - 1; i++)
            {
                autocompletedText += queryWords[i] + ' ';
            }
            autocompletedText += AutocompleteList.Adapter.GetItem(e.Position).ToString();
            text.Text = autocompletedText;
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
                for(int i  = 0; i < prompts.Length; i++)
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
                pageid = 1;
                AutocompleteList.Visibility = ViewStates.Invisible;
                Output.Text = "";
                if (Container.ChildCount > 0)
                    Container.RemoveAllViews();
                string query = text.Text.Replace(" ", "+");
                HttpWebRequest request = WebRequest.CreateHttp($"https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&limit={pageResultLimit}&tags={query}&pid={(pageid - 1)*pageResultLimit}");

                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

                StreamReader responseReader = new StreamReader(response.GetResponseStream());

                string responseString = responseReader.ReadToEnd();

                if (responseString.IndexOf("sample_url=\"") < 0)
                {
                    Toast.MakeText(this, "Not found any image🤷‍♂️", ToastLength.Short).Show();
                    return;
                }

                await Xamarin.Essentials.Clipboard.SetTextAsync(responseString);
                string[] picturesUrls = new string[responseString.Split("sample_url=").Length -1];
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
                        byte[] bytesForImage = client.DownloadData(picturesUrls[i]);
                        
                        Bitmap Picture = BitmapFactory.DecodeByteArray(bytesForImage, 0, bytesForImage.Length);
                        ImageView image = new ImageView(this);
                        
                        if (Picture != null)
                        {
                            
                            image.SetPadding(0, 10, 0, 10);

                            image.SetImageBitmap(Picture);

                            int height = Picture.Height / Picture.Width * Container.Width;

                            image.LayoutParameters = new ViewGroup.LayoutParams(Container.Width, ViewGroup.LayoutParams.WrapContent);

                            image.SetScaleType(ImageView.ScaleType.FitXy);
                        image.SetAdjustViewBounds(true);
                        
                            
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
                    //Toast.MakeText(this, $"Setting {imageViews[i].Height.ToString()}", ToastLength.Short).Show();

                    Container.AddView(imageViews[i]);
                }





            }
            catch (Exception ex)
            {
                Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(this);
                builder.SetTitle("Oops");
                builder.SetMessage("Something went wrong. If this is not the first time you get error try to search by other tags");
                builder.SetPositiveButton("Ok", (sender, eventargs) => { });
                builder.SetNeutralButton("Copy error message", (sender, eventargs ) => { Xamarin.Essentials.Clipboard.SetTextAsync(ex.Message + " | " + ex.StackTrace); });
                builder.Create().Show();
            }
        }
    }
}