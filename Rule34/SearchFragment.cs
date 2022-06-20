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

using BooruBrowser.Api;


namespace BooruBrowser
{
    public class SearchFragment : AndroidX.Fragment.App.Fragment
    {
        private AutoCompleteTextView text;
        private RelativeLayout relativeLayout;
        private int pageResultLimit = 1000;
        private int pageIndex = 0;
        private string lastQuery;

        private Button NextPageButton;
        private Button PreviousPageButton;
        private LinearLayout Paginator;
        private TextView PageNumberIndicator;
        private ProgressBar progressBar1;
        private Spinner sortBySpinner;
        private Spinner sortOrderSpinner;

        private string searchSortBy = "updated";
        private string sortOrder = "desc";

        private ListPopupWindow autocompleteListWindow;

        private Android.Support.V7.Widget.RecyclerView recyclerView;

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
            //Activity.FindViewById<ImageView>(Resource.Id.top_booru_logo).SetImageResource(Resource.Drawable.topb);
            relativeLayout = Activity.FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
            NextPageButton.Click += NextPageButton_Click;
            PreviousPageButton.Click += PreviousPageButton_Click;
            PageNumberIndicator.SetMinimumWidth(PreviousPageButton.MinimumWidth);

            Paginator.Visibility = ViewStates.Gone;

            

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

            text.ShowSoftInputOnFocus = true;

            text.RequestFocus();

            recyclerView = Activity.FindViewById<Android.Support.V7.Widget.RecyclerView>(Resource.Id.search_fragment_recyclerview);
            recyclerView.SetLayoutManager(new Android.Support.V7.Widget.LinearLayoutManager(Activity));
            recyclerView.NestedScrollingEnabled = false;
            
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


        
        private async void SearchNew()
        {
            if ($"{GetSortQuery()}+{text.Text}" == lastQuery)
                return;
            autocompleteListWindow.Dismiss();
            lastQuery = $"{GetSortQuery()}+{text.Text}";
            Paginator.Visibility = ViewStates.Gone;
            PreviousPageButton.Enabled = false;
            pageIndex = 0;
            await Search();
        }

        private async void PreviousPageButton_Click(object sender, EventArgs e)
        {
            pageIndex--;
            await Search();
        }

        private async void NextPageButton_Click(object sender, EventArgs e)
        {
            pageIndex++;
            await Search();
        }

        private void AutocompleteList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var windowSender = sender as ListPopupWindow;
            List<string> tags = text.Text.Split(' ').SkipLast(1).ToList();

            tags.Add((windowSender.ListView.Adapter as AutocompleteListAdapter).GetItem(e.Position).TagValue);
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

                    var autocompletes = GelbooruApi.Autocomplete(query);
                    if (autocompletes.Length > 0)
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
                
                string query = lastQuery.Replace(' ', '+');

                var searchResult = await GelbooruApi.SearchPosts(query.Split('+'), pageIndex, pageResultLimit);//PostsCollection.FromXml(response);
                var Collection = searchResult.Posts;
                List<PostThumbnail> postThumbnails = new List<PostThumbnail>();

                if (Collection.Count < 1)
                {
                    Toast.MakeText(Activity, "Not found any image🤷‍♂️", ToastLength.Short).Show();
                    return;
                }

                SearchRecyclerViewAdapter searchRecyclerViewAdapter = new SearchRecyclerViewAdapter(Collection.ToArray());
               
                recyclerView.SetAdapter(searchRecyclerViewAdapter);
                

                searchRecyclerViewAdapter.HolderClick += (object sender, Android.Support.V7.Widget.RecyclerView.ViewHolder holder) =>
                {
                    Toast.MakeText(Activity, "Item clicked", ToastLength.Short).Show();
                    //isWorking = false;
                    //Task.Delay(300).Wait();
                    //new Handler(Activity.MainLooper).Post(() =>
                    //{
                    //    var transaction = ParentFragmentManager.BeginTransaction();
                    //    PostFragment fragment = new PostFragment(Preview);
                    //    transaction.Replace(Resource.Id.main_frame_layout, fragment);
                    //    transaction.SetReorderingAllowed(true);
                    //    transaction.AddToBackStack("post_fragment");
                    //    transaction.Commit();

                    //});

                };
                searchRecyclerViewAdapter.HolderLongClick += (sender, e) =>
                {

                    var searchrvholder = e as SearchRecyclerViewAdapterViewHolder;

                    AndroidX.AppCompat.App.AlertDialog.Builder builder = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity);
                    builder.SetTitle("Download");
                    builder.SetMessage("Do you want to download the content of this post?");
                    builder.SetPositiveButton("Yes", (object sender, Android.Content.DialogClickEventArgs e) =>
                    {

                        DownloadManager manager = DownloadManager.FromContext(Activity);
                        DownloadManager.Request downloadRequest = new DownloadManager.Request(Android.Net.Uri.Parse(searchrvholder.Post.FileUrl));

                        downloadRequest.SetNotificationVisibility(DownloadVisibility.VisibleNotifyCompleted);
                        downloadRequest.SetTitle(searchrvholder.Post.Tags[0]);
                        downloadRequest.SetDestinationInExternalPublicDir(Android.OS.Environment.DirectoryDownloads, $"{searchrvholder.Post.Tags[0]}" +
                            $"{searchrvholder.Post.FileUrl.Substring(searchrvholder.Post.FileUrl.LastIndexOf('.'))}");

                        new Handler(Activity.MainLooper).Post(() =>
                        {
                            long id = manager.Enqueue(downloadRequest);

                        });
                    });
                    builder.SetNegativeButton("No", (object sender, Android.Content.DialogClickEventArgs e) => { });
                    AndroidX.AppCompat.App.AlertDialog dialog = builder.Create();
                    //dialog.Show();

                    string[] popupOptions = new string[] { "Download", "Vote Up", "Share" };
                    ListPopupWindow popupwindow = new ListPopupWindow(Activity);
                    popupwindow.SetAdapter(new ArrayAdapter<string>(Activity, Android.Resource.Layout.SimpleListItem1, popupOptions));
                    popupwindow.AnchorView = searchrvholder.ItemView;
                    popupwindow.Modal = false;
                    popupwindow.ItemClick += (s, e) =>
                    {
                        var popup = s as ListPopupWindow;
                        popup.Dismiss();
                        

                        switch (e.Position)
                        {
                            case 0:
                                dialog.Show();
                                break;

                            case 1:
                                int votesUpdated = 0;//BooruBrowserApi.VoteUp(postThumb.GetPost().postId);
                                if (searchrvholder.Post.Score != votesUpdated)
                                {
                                    Toast.MakeText(Activity, $"Voted! The score of this post now: {votesUpdated}", ToastLength.Short).Show();
                                    searchrvholder.Post.Score = votesUpdated;
                                }
                                else
                                {
                                    Toast.MakeText(Activity, $"You have already voted up this post", ToastLength.Short).Show();
                                }
                                break;

                            case 2:
                                Intent shareIntent = new Intent();
                                shareIntent.SetAction(Intent.ActionSend);
                                shareIntent.PutExtra(Intent.ExtraText, $"https://gelbooru.com/index.php?page=post&s=view&id={searchrvholder.Post.PostId}");
                                shareIntent.SetType("text/plain");

                                Intent secint = Intent.CreateChooser(shareIntent, "Share post");
                                StartActivity(secint);
                                break;

                        }


                    };
                    popupwindow.Show();
                };


                UpdatePaginator(searchResult.TotalPostsCount, searchResult.Offset, pageResultLimit);
                Paginator.Visibility = ViewStates.Visible;

#if DEBUG
                stopwatch.Stop();
                //Toast.MakeText(Activity, "Page load time: " + stopwatch.ElapsedMilliseconds.ToString(), ToastLength.Short).Show();
#endif
            }
            catch (AggregateException ex)
            {
                string innerex = string.Empty;

                foreach (var exitem in ex.InnerExceptions)
                {
                    innerex += exitem.Message + '\n';
                }

                AndroidX.AppCompat.App.AlertDialog.Builder builder = new AndroidX.AppCompat.App.AlertDialog.Builder(Activity);

                builder.SetTitle("Oops");
                builder.SetMessage("Something went wrong. If this is not the first time you get error try to search by other tags");
                builder.SetPositiveButton("Ok", (sender, eventargs) => { });
                builder.SetNeutralButton("Copy error message", (sender, eventargs) => { Xamarin.Essentials.Clipboard.SetTextAsync(innerex); });
                builder.Create().Show();
            }
        }

        public void UpdatePaginator(int totalPosts, int offset, int limit)
        {
            int thisPageNumber = offset / limit + 1;

            PageNumberIndicator.Text = thisPageNumber.ToString();

            if (offset + limit < totalPosts)
            {
                NextPageButton.Enabled = true;
            }
            else
            {
                NextPageButton.Enabled = false;
            }

            if (offset == 0)
            {
                PreviousPageButton.Enabled = false;
            }
            else
            {
                PreviousPageButton.Enabled = true;
            }

        }

        public override void OnPause()
        {
            base.OnPause();
            Toast.MakeText(Context, "Search fragment paused", ToastLength.Short).Show();
            Toast.MakeText(Context, this.IsResumed.ToString(), ToastLength.Short).Show();
        }

        public override void OnResume()
        {
            base.OnResume();
            Toast.MakeText(Context, "Search fragment resumed", ToastLength.Short).Show();

        }

    }


}
