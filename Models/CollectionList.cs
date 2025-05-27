using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionManagerApp.Models
{
    public class CollectionList
    {
        public string Name { get; set; }
        public ObservableCollection<Item> Items { get; set; } = new();

        public void UpdateImageNumbersAfterItemDeletion(Item deletedItem)
        {
            string imagesFolder = Path.Combine(FileSystem.AppDataDirectory, "Images");
            //string imagesFolder = Path.GetDirectoryName(deletedItem.ImagePath)!;
            string collectionPrefix = $"{Name}_";
            int deletedIndex = int.Parse(Path.GetFileNameWithoutExtension(deletedItem.ImagePath).Substring(collectionPrefix.Length));
            List<Item> higherNumberItems = Items
                .Where(i => !string.IsNullOrEmpty(i.ImagePath))
                .Where(i => int.TryParse(Path.GetFileNameWithoutExtension(i.ImagePath).Substring(collectionPrefix.Length), out int itemImageIndex) && itemImageIndex > deletedIndex)
                .OrderBy(i => int.Parse(Path.GetFileNameWithoutExtension(i.ImagePath).Substring(collectionPrefix.Length)))
                .ToList();

            for (int i = 0; i < higherNumberItems.Count; i++)
            {
                int imageIndex = int.Parse(Path.GetFileNameWithoutExtension(higherNumberItems[i].ImagePath).Substring(collectionPrefix.Length));
                string newPath = Path.Combine(imagesFolder, $"{collectionPrefix}{imageIndex-1}{Path.GetExtension(higherNumberItems[i].ImagePath)}");
                if (File.Exists(higherNumberItems[i].ImagePath))
                {
                    File.Move(higherNumberItems[i].ImagePath, newPath);
                    higherNumberItems[i].ImagePath = newPath;
                }
            }
        }
        //public void UpdateImageNumbersAfterItemDeletion(string deletedImagePath)
        //{
        //    string imagesFolder = Path.GetDirectoryName(deletedImagePath)!;
        //    string collectionPrefix = $"{Name}_";

        //    var imageFiles = Directory.GetFiles(imagesFolder, $"{collectionPrefix}*")
        //                              .Where(f => Path.GetExtension(f).ToLower() is ".jpg" or ".jpeg" or ".png")
        //                              .OrderBy(f => f)
        //                              .ToList();

        //    int deletedIndex = -1;
        //    string fileName = Path.GetFileNameWithoutExtension(deletedImagePath);
        //    if (fileName.StartsWith(collectionPrefix))
        //    {
        //        string numberPart = fileName.Substring(collectionPrefix.Length);
        //        if (int.TryParse(numberPart, out int number))
        //            deletedIndex = number;
        //    }

        //    for (int i = deletedIndex; i <= imageFiles.Count; i++)
        //    {
        //        string oldPath = Path.Combine(imagesFolder, $"{collectionPrefix}{i + 1}{Path.GetExtension(imageFiles[i-1])}");
        //        string newPath = Path.Combine(imagesFolder, $"{collectionPrefix}{i}{Path.GetExtension(imageFiles[i-1])}");

        //        if (File.Exists(oldPath))
        //        {
        //            File.Move(oldPath, newPath);
        //            Item? itemToUpdate = Items.FirstOrDefault(i => i.ImagePath == oldPath);
        //            if (itemToUpdate != null)
        //                itemToUpdate.ImagePath = newPath;
        //        }
        //    }
        //}
    }
}
