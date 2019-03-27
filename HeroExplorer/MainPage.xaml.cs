using HeroExplorer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HeroExplorer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<Character> MarvelCharacters { get; set; }
        public ObservableCollection<ComicBook> MarvelComics { get; set; }
        public ObservableCollection<Character> searchedMarvelCharacters { get; set; }
        private List<string> TotalSuggestions;
        private List<string> ChangingSuggestions;

        public MainPage()
        {
            this.InitializeComponent();          
            MarvelCharacters = new ObservableCollection<Character>();
            MarvelComics = new ObservableCollection<ComicBook>();
            searchedMarvelCharacters = new ObservableCollection<Character>();
            TotalSuggestions = new List<string>();
            CharacterText.Visibility = Visibility.Collapsed;
            SearchAutoSuggestBox.Visibility = Visibility.Collapsed;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //var storageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(
            //    new Uri("ms-appx:///VoiceCommandDictionary.xml"));
            //await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager
            //    .InstallCommandDefinitionsFromStorageFileAsync(storageFile);
            MarvelCopywrite.Text = await MarvelFacade.GetAttributionTextAsync();
            Refresh();
        }

        public async void Refresh()
        {
            MyProgressRing.IsActive = true;
            MyProgressRing.Visibility = Visibility.Visible;

            MarvelCharacters.Clear();
            while (MarvelCharacters.Count < 10)
            {                
                Task t = MarvelFacade.PopulateMarvelCharactersAsync(MarvelCharacters);
                await t;
            }

            CharactersText.Text = "Characters";
            MasterListView.ItemsSource = MarvelCharacters;

            MyProgressRing.IsActive = false;
            MyProgressRing.Visibility = Visibility.Collapsed;
            
            SearchAutoSuggestBox.Visibility = Visibility.Visible;
        }

        private async void MasterListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            MyProgressRing.IsActive = true;
            MyProgressRing.Visibility = Visibility.Visible;

            ComicDetailDescriptionTextBlock.Text = "";
            ComicDetailNameTextBlock.Text = "";
            ComicDetailImage.Source = null;

            var selectedCharacter = (Character)e.ClickedItem;

            DetailNameTextBlock.Text = selectedCharacter.name;
            DetailDescriptionTextBlock.Text = selectedCharacter.description;

            var largeImage = new BitmapImage();
            Uri uri = new Uri(selectedCharacter.thumbnail.large, UriKind.Absolute);
            largeImage.UriSource = uri;
            DetailImage.Source = largeImage;

            MarvelComics.Clear();

            CharacterText.Text = "Charcacter Details";
            CharacterText.Visibility = Visibility.Visible;

            await MarvelFacade.PopulateMarvelComicsAsync(
                selectedCharacter.id,
                MarvelComics);

            ComicsGridView.ItemsSource = MarvelComics;           

            MyProgressRing.IsActive = false;
            MyProgressRing.Visibility = Visibility.Collapsed;

        }

        private void ComicsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedComic = (ComicBook)e.ClickedItem;

            ComicDetailNameTextBlock.Text = selectedComic.title;

            if (selectedComic.description != null)
                ComicDetailDescriptionTextBlock.Text = selectedComic.description;

            var largeImage = new BitmapImage();
            Uri uri = new Uri(selectedComic.thumbnail.large, UriKind.Absolute);
            largeImage.UriSource = uri;
            ComicDetailImage.Source = largeImage;
        }

        private void SearchAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            ChangingSuggestions = TotalSuggestions.Where(p => p.StartsWith(sender.Text)).ToList();
            SearchAutoSuggestBox.ItemsSource = ChangingSuggestions.Reverse<string>();
        }

        private async void SearchAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var isFound = false;

            if (TotalSuggestions.Count > 0)
            {
                foreach (var suggestion in TotalSuggestions)
                {
                    if (suggestion == sender.Text)
                    {
                        isFound = true;
                    }
                }
            }

            if (isFound == false)
            {
                TotalSuggestions.Add(sender.Text);
            }

            MyProgressRing.IsActive = true;
            MyProgressRing.Visibility = Visibility.Visible;

            searchedMarvelCharacters.Clear();

            DetailNameTextBlock.Text = "";
            DetailDescriptionTextBlock.Text = "";
            DetailImage.Source = null;
            ComicsGridView.ItemsSource = null;

            ComicDetailDescriptionTextBlock.Text = "";
            ComicDetailNameTextBlock.Text = "";
            ComicDetailImage.Source = null;

            await MarvelFacade.PopulateSearchedMarvilCharacterAsync(sender.Text, searchedMarvelCharacters);

            MasterListView.ItemsSource = searchedMarvelCharacters;

            if (searchedMarvelCharacters.Count == 0)
            {                
                CharacterText.Text = "CHARACTER NOT FOUND!";
                CharactersText.Text = "";
            }
            else
            {
                CharacterText.Text = "";
                CharactersText.Text = "Characters";
            }
            

            MyProgressRing.IsActive = false;
            MyProgressRing.Visibility = Visibility.Collapsed;

        }
    }
}
