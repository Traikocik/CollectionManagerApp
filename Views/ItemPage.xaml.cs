using CollectionManagerApp.Models;

namespace CollectionManagerApp.Views;

public partial class ItemPage : ContentPage
{
    private CollectionList parentCollectionList;
    private bool isEditing;
    private FileResult? newImageFileResult = null;
    private string imagePath = "";

    public ItemPage(CollectionList parentCollectionList, Item? itemToEdit = null)
    {
        InitializeComponent();
        ConditionPicker.ItemsSource = new string[] { "new", "used", "for sale", "sold", "want to buy" };
        ConditionPicker.SelectedIndex = 0;
        this.parentCollectionList = parentCollectionList;
        this.isEditing = itemToEdit != null;

        if (isEditing)
        {
            BindingContext = itemToEdit;
            NameEntry.Text = itemToEdit.Name;
            Title = itemToEdit.Name;
            ConditionPicker.SelectedItem = itemToEdit.Condition;
            PriceEntry.Text = itemToEdit.Price.ToString();
            RatingSlider.Value = itemToEdit.Rating;
            CommentEntry.Text = itemToEdit.Comment;
            if (!string.IsNullOrEmpty(itemToEdit.ImagePath))
            {
                imagePath = itemToEdit.ImagePath;
                ItemImage.Source = ImageSource.FromFile(itemToEdit.ImagePath);
                ItemImageLayout.IsVisible = true;
            }
        }
    }

    private async void PickImageBtn_Clicked(object sender, EventArgs e)
    {
        FileResult? result = await FilePicker.PickAsync(new PickOptions
        {
            FileTypes = FilePickerFileType.Images,
            PickerTitle = "Pick item's image"
        });

        if (result != null)
        {
            newImageFileResult = result;
            ItemImage.Source = ImageSource.FromFile(result.FullPath);
            ItemImageLayout.IsVisible = true;
        }
    }

    private async void SaveBtn_Clicked(object sender, EventArgs e)
    {
        string name = NameEntry.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlert("ERROR", "Item's name is wrong!", "OK");
            return;
        }
        if (!isEditing && parentCollectionList.Items.Any(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            bool confirm = await DisplayAlert("WARNING", $"Item with name: '{name}' already exists.\nDo you still want to save this item?", "Yes", "No");
            if (!confirm)
                return;
        }

        string condition = (string)ConditionPicker.SelectedItem;

        if (!double.TryParse(PriceEntry.Text, out double price))
        {
            await DisplayAlert("ERROR", "Item's price is wrong!", "OK");
            return;
        }

        int rating = (int)RatingSlider.Value;
        string comment = CommentEntry.Text ?? "";

        if (newImageFileResult != null)
        {
            string ImagesFolder = Path.Combine(FileSystem.AppDataDirectory, "Images");
            if (!Directory.Exists(ImagesFolder))
                Directory.CreateDirectory(ImagesFolder);

            imagePath = Path.Combine(ImagesFolder, newImageFileResult.FileName);
            using Stream stream = await newImageFileResult.OpenReadAsync();
            using FileStream fileStream = File.Create(imagePath);
            await stream.CopyToAsync(fileStream);

            if (isEditing)
            {
                string oldImagePath = ((Item)BindingContext).ImagePath;
                if (!string.IsNullOrEmpty(oldImagePath) && File.Exists(oldImagePath))
                    File.Delete(oldImagePath);
            }

            ItemImageLayout.IsVisible = true;
            ItemImage.Source = ImageSource.FromFile(imagePath);
        }
        else if (BindingContext is Item { ImagePath: string originalImagePath } && !string.IsNullOrEmpty(originalImagePath) && File.Exists(originalImagePath))
            File.Delete(originalImagePath);

        if (isEditing)
        {
            ((Item)BindingContext).Name = name;
            ((Item)BindingContext).Condition = condition;
            ((Item)BindingContext).Price = price;
            ((Item)BindingContext).Rating = rating;
            ((Item)BindingContext).Comment = comment;
            ((Item)BindingContext).ImagePath = imagePath;
        }
        else
        {
            parentCollectionList.Items.Add(new Item
            {
                Name = name,
                Condition = condition,
                Price = price,
                Rating = rating,
                Comment = comment,
                ImagePath = imagePath
            });
        }

        AllCollectionLists.SaveCollectionListToFile(parentCollectionList);
        await Navigation.PopAsync();
    }

    private void RatingSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        ((Slider)sender).Value = (int)Math.Round(e.NewValue);
    }

    private void DeleteImageBtn_Clicked(object sender, EventArgs e)
    {
        newImageFileResult = null;
        imagePath = "";
        ItemImage.Source = null;
        ItemImageLayout.IsVisible = false;
    }
}