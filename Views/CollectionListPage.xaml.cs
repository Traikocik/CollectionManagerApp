using CollectionManagerApp.Models;

namespace CollectionManagerApp.Views;

public partial class CollectionListPage : ContentPage
{
    public CollectionListPage(CollectionList selectedCollection)
    {
        InitializeComponent();
        Title = selectedCollection.Name;
        BindingContext = selectedCollection;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SeparateSoldItems();
        ItemListView.ItemsSource = null;
        ItemListView.ItemsSource = ((CollectionList)BindingContext).Items;
    }

    private void SeparateSoldItems()
    {
        List<Item> sortedItems = [.. ((CollectionList)BindingContext).Items.OrderBy(item => item.Condition == "sold")];
        ((CollectionList)BindingContext).Items.Clear();
        foreach (Item item in sortedItems)
            ((CollectionList)BindingContext).Items.Add(item);
    }

    private async void AddItemBtn_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ItemPage((CollectionList)BindingContext));
    }

    private async void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count != 0)
        {
            await Navigation.PushAsync(new ItemPage((CollectionList)BindingContext, (Item)e.CurrentSelection[0]));
            ItemListView.SelectedItem = null;
        }
    }

    private void DeleteItemBtn_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton { BindingContext: Item item })
        {
            ((CollectionList)BindingContext).Items.Remove(item);
            if (!string.IsNullOrEmpty(item.ImagePath) && File.Exists(item.ImagePath))
            {
                File.Delete(item.ImagePath);
                ((CollectionList)BindingContext).UpdateImageNumbersAfterItemDeletion(item);
            }
            AllCollectionLists.SaveCollectionListToFile((CollectionList)BindingContext);
        }
    }

    private async void SummaryBtn_Clicked(object sender, EventArgs e)
    {
        int total = ((CollectionList)BindingContext).Items.Count;
        int sold = ((CollectionList)BindingContext).Items.Count(i => i.Condition == "sold");
        int forSale = ((CollectionList)BindingContext).Items.Count(i => i.Condition == "for sale");

        await DisplayAlert("Collection summary", $"All items: {total}\nSold items: {sold}\nItems for sale: {forSale}", "OK");
    }

    private async void ExportCollectionBtn_Clicked(object sender, EventArgs e)
    {
        string path = await AllCollectionLists.ExportCollectionList((CollectionList)BindingContext);
        if (!string.IsNullOrEmpty(path))
            await DisplayAlert("EXPORTED", $"Exported to:\n{path}", "OK");
        else
            await DisplayAlert("ERROR", $"Something went wrong during export", "OK");
    }

    private void ItemLayout_Loaded(object sender, EventArgs e)
    {
        if (sender is StackLayout itemLayout)
        {
            itemLayout.BackgroundColor = ((Item)itemLayout.BindingContext).Condition == "sold" ? Color.FromHex("#60656E") : Color.FromHex("#5081D9");
        }
    }
}