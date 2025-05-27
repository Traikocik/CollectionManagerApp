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
    }
}
