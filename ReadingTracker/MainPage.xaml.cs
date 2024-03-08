using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReadingTracker.Requests;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace ReadingTracker
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        private ObservableCollection<string> recommendedSearches = new ObservableCollection<string>();

        public MainPage()
        {
            InitializeComponent();
            searchBar.SearchButtonPressed += SearchBar_SearchButtonPressed;
            searchBar.TextChanged += SearchBar_TextChanged;
            searchBar.Unfocused += SearchBar_Unfocused;

            searchErrorLabel.IsVisible = false;
            searchResults.IsVisible = false;

            recommendedListView.ItemsSource = recommendedSearches;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            AppShell.SetBackgroundColor(this, Color.FromRgb(255, 255, 255));
            AppShell.SetForegroundColor(this, Color.FromRgb(255, 255, 255));
        }

        private string GetAuthor(JToken doc)
        {
            JToken authorToken = doc["author_name"];
            if (authorToken != null)
            {
                if (authorToken.Type == JTokenType.Array)
                {
                    var authorsArray = authorToken.ToObject<string[]>();
                    return authorsArray.Length > 0 ? authorsArray[0] : "Unknown Author";
                }
                else
                {
                    return authorToken.ToObject<string>();
                }
            }
            return "Unknown Author";
        }

        private string GetFormattedPublishDate(JToken token)
        {
            JArray dateArray = token as JArray;
            if (dateArray != null && dateArray.Count > 0)
            {
                foreach (JToken dateToken in dateArray)
                {
                    string date = dateToken?.ToObject<string>();
                    if (!string.IsNullOrEmpty(date))
                    {
                        Match match = Regex.Match(date, @"\b\d{4}\b");
                        if (match.Success)
                        {
                            return match.Value;
                        }
                        else
                        {
                            return "Unknown Publish Date";
                        }
                    }
                    else
                    {
                        return "Unknown Publish Date";
                    }
                }
            }
            return "Unknown Publish Date";
        }

        private async void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            searchErrorLabel.IsVisible = false;
            searchResults.IsVisible = true;

            string searchQuery = searchBar.Text;
            loadingIndicator.IsVisible = true;
            loadingIndicator.IsRunning = true;
            var result = await RequestHandler.SearchBooks(searchQuery);
            loadingIndicator.IsVisible = false;
            loadingIndicator.IsRunning = false;

            dynamic jsonResponse = JsonConvert.DeserializeObject(result);
            int numResults = jsonResponse["numFound"];

            searchStackLayout.Children.Clear();

            if (numResults == 0)
            {
                searchErrorLabel.IsVisible=true;
            }

            if (numResults > 0) 
            {
                searchErrorLabel.IsVisible = true;
                searchErrorLabel.Text = $"Results found: {numResults}";

                JArray docs = (JArray)jsonResponse["docs"];
                foreach (JToken doc in docs)
                {
                    string title = GetStringFromToken(doc, "title");
                    string author = GetStringFromToken(doc, "author_name");
                    string publishDate = GetFormattedPublishDate(doc["publish_date"]);
                    string coverUrl = $"https://covers.openlibrary.org/b/id/{doc["cover_i"]}-L.jpg";

                    AddBookFrame(title, author, publishDate, coverUrl);
                }

            }

            reccommended.IsVisible = false;
            searchResults.IsVisible = true;
        }

        private string GetStringFromToken(JToken token, string propertyName)
        {
            JToken propertyToken = token[propertyName];
            if (propertyToken != null)
            {
                if (propertyToken.Type == JTokenType.Array)
                {
                    // If it's an array, convert it to a comma-separated string
                    return string.Join(", ", propertyToken.Values<string>());
                }
                else
                {
                    // If it's not an array, directly convert it to a string
                    return propertyToken.ToObject<string>();
                }
            }
            return "Unknown"; // Return default value if property is not found
        }

        private void AddBookFrame(string title, string author, string publishDate, string coverUrl)
        {

            Frame frame = new Frame
            {
                Padding = new Thickness(20),
                Margin = new Thickness(10, 10, 10, 10),
                BackgroundColor = Color.FromArgb("#F5F5F5"),
                CornerRadius = 12,
                BorderColor = Color.FromRgba(0, 0, 0, 0),
                HasShadow = false
            };

            StackLayout innerStackLayout = new StackLayout();

            // Title
            Label titleLabel = new Label
            {
                Text = title,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };
            innerStackLayout.Children.Add(titleLabel);

            // Author
            Label authorLabel = new Label
            {
                Text = $"By {author}",
                TextColor= Color.FromArgb("#808080"),
                Margin = new Thickness(0, 0, 0, 5)
            };
            innerStackLayout.Children.Add(authorLabel);

            // Publish Date
            Label publishDateLabel = new Label
            {
                Text = $"Publish Date: {publishDate}",
                TextColor = Color.FromArgb("#808080"),
                Margin = new Thickness(0, 0, 0, 5)
            };
            innerStackLayout.Children.Add(publishDateLabel);

            // Cover Image
            if (!string.IsNullOrEmpty(coverUrl))
            {
                Image coverImage = new Image
                {
                    Source = coverUrl,
                    Aspect = Aspect.AspectFit,
                    HeightRequest = 200,
                    Margin = new Thickness(0, 10, 0, 5)
                };
                innerStackLayout.Children.Add(coverImage);
            }
            else
            {
                Image coverImage = new Image
                {
                    Source = null,
                    Aspect = Aspect.AspectFit,
                    HeightRequest = 0,
                    Margin = new Thickness(0, 0, 0, 0)
                };
                innerStackLayout.Children.Add(coverImage);
            }            

            Button addToList = new Button
            {
                Text = "Add to My List",
                Margin = new Thickness(0, 10, 0, 0),
                BackgroundColor = Color.FromRgba(0, 0, 0, 255),
                TextColor = Color.FromRgba(255, 255, 255, 255),
                FontAttributes = FontAttributes.Bold,
            };
            innerStackLayout.Children.Add(addToList);

            Button moreInfo = new Button
            {
                Text = "More Information",
                Margin = new Thickness(0, 10, 0, 0),
                BackgroundColor = Color.FromRgba(255, 255, 255, 255),
                TextColor = Color.FromRgba(0, 0, 0, 255),
                FontAttributes = FontAttributes.Bold,
            };
            innerStackLayout.Children.Add(moreInfo);

            frame.Content = innerStackLayout;
            searchStackLayout.Children.Add(frame);
        }

        private async void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue;
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                // Simulated book suggestions based on the user input
                List<string> suggestedBooks = await GetBookSuggestions(searchText);

                // Update the recommended searches with the suggested books
                recommendedSearches.Clear();
                foreach (string book in suggestedBooks)
                {
                    recommendedSearches.Add(book);
                }

                recommendedListView.IsVisible = true;
            }
            else
            {
                recommendedSearches.Clear();
                recommendedListView.IsVisible = false;
            }
        }

        private void RecommendedListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                // Get the selected item's text
                string selectedItemText = e.SelectedItem.ToString();

                // Set the text of the search bar
                searchBar.Text = selectedItemText;

                // Deselect the selected item
                recommendedListView.SelectedItem = null;
            }
        }

        private void SearchBar_Unfocused(object sender, FocusEventArgs e)
        {
            // Hide the ListView when the search bar loses focus
            recommendedListView.IsVisible = false;
            searchBar.Unfocus();
        }

        private async Task<List<string>> GetBookSuggestions(string searchText)
        {

            List<string> suggestedBooks = await RequestHandler.GetBookNames(searchText);
            foreach (string bookName in suggestedBooks)
            {
                Console.WriteLine(bookName);
            }

            suggestedBooks = suggestedBooks.Where(book => book.ToLower().Contains(searchText.ToLower())).ToList();

            return suggestedBooks;
        }

        private async void ShowBookPopup(string title, string author, string publishDate, List<string> genres)
        {
           
        }

    }
}