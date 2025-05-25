using CollectionManagerApp.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Storage;

namespace CollectionManagerApp.Views;

public partial class AllCollectionListsPage : ContentPage
{
    public AllCollectionListsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AllCollectionLists.LoadAllCollectionLists();
        CollectionListView.ItemsSource = AllCollectionLists.CollectionLists;
    }

    private async void CollectionListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count != 0)
        {
            await Navigation.PushAsync(new CollectionListPage((CollectionList)e.CurrentSelection[0]));
            CollectionListView.SelectedItem = null;
        }
    }

    private async void AddCollectionBtn_Clicked(object sender, EventArgs e)
    {
        string newCollectionName = await DisplayPromptAsync("New collection list", 
            "Enter name of the new collection:", 
            "Create",
            "Cancel",
            "New collection name",
            100,
            Keyboard.Text);
        if (string.IsNullOrWhiteSpace(newCollectionName))
        {
            await DisplayAlert("WARNING", "Collection list's name was not given, collection list can not be created!", "OK");
            return;
        }

        if (AllCollectionLists.CollectionLists.Any(c => c.Name.Equals(newCollectionName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            await DisplayAlert("WARNING!", "Collection list with entered name already exists.", "OK");
            return;
        }

        CollectionList newCollection = new() { Name = newCollectionName.Trim() };
        AllCollectionLists.CollectionLists.Add(newCollection);
        AllCollectionLists.SaveCollectionListToFile(newCollection);
    }

    private async void ImportCollectionBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            FolderPickerResult? folderResult = await FolderPicker.Default.PickAsync();
            if (folderResult == null)
            {
                await DisplayAlert("ERROR", "Something went wrong while selecting the folder!", "OK");
                return;
            }
            if(folderResult.Folder == null)
            {
                await DisplayAlert("ERROR", "No folder was picked!", "OK");
                return;
            }
            string folderPath = folderResult.Folder.Path;

            string importErrorOutput = AllCollectionLists.ImportCollectionListFromFolder(folderPath);
            if (!string.IsNullOrEmpty(importErrorOutput))
            {
                await DisplayAlert("ERROR", importErrorOutput, "OK");
                return;
            }
            AllCollectionLists.SaveAllCollectionLists();
            await DisplayAlert("SUCCESS", "Import ended successfully.", "OK");
        }
        catch
        {
            await DisplayAlert("ERROR", "Something went wrong while importing collection list.", "OK");
        }
    }
}