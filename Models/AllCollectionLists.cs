using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Storage;

namespace CollectionManagerApp.Models
{
    internal class AllCollectionLists
    {
        public static ObservableCollection<CollectionList> CollectionLists { get; set; } = new();
        private static readonly string CollectionsFolder = Path.Combine(FileSystem.AppDataDirectory, "Collections");

        public static void LoadAllCollectionLists()
        {
            CollectionLists.Clear();

            if (!Directory.Exists(CollectionsFolder))
                Directory.CreateDirectory(CollectionsFolder);

            foreach (string file in Directory.GetFiles(CollectionsFolder, "*.txt"))
            {
                CollectionList? collection = LoadCollectionListFromFile(file);
                if (collection != null)
                    CollectionLists.Add(collection);
            }
        }

        public static void SaveAllCollectionLists()
        {
            if (!Directory.Exists(CollectionsFolder))
                Directory.CreateDirectory(CollectionsFolder);

            foreach (CollectionList collection in CollectionLists)
            {
                SaveCollectionListToFile(collection);
            }
        }

        public static CollectionList? LoadCollectionListFromFile(string path)
        {
            try
            {
                string[] lines = File.ReadAllLines(path);
                if (lines.Length == 0)
                    return null;

                CollectionList collection = new() { Name = lines[0] };

                for (int i = 1; i < lines.Length;)
                {
                    if (lines[i] == "[ITEM]")
                    {
                        if (i + 6 >= lines.Length)
                            break;

                        Item item = new Item
                        {
                            Name = lines[i + 1],
                            Condition = lines[i + 2],
                            Price = decimal.Parse(lines[i + 3]),
                            Rating = int.Parse(lines[i + 4]),
                            Comment = lines[i + 5].Replace("\\n", Environment.NewLine),
                            ImagePath = lines[i + 6] // string.IsNullOrEmpty(lines[i + 6]) ? "" : lines[i + 6]
                        };

                        collection.Items.Add(item);
                        i += 7;
                    }
                    else
                    {
                        i++;
                    }
                }

                return collection;
            }
            catch
            {
                return null;
            }
        }

        public static void SaveCollectionListToFile(CollectionList collection)
        {
            string path = Path.Combine(CollectionsFolder, $"{collection.Name}.txt");
            using StreamWriter writer = new(path);
            writer.WriteLine(collection.Name);
            foreach (Item item in collection.Items)
            {
                writer.WriteLine("[ITEM]");
                writer.WriteLine(item.Name);
                writer.WriteLine(item.Condition);
                writer.WriteLine(item.Price);
                writer.WriteLine(item.Rating);
                writer.WriteLine(item.Comment);
                writer.WriteLine(item.ImagePath ?? "");
            }
        }

        public static async Task<string> ExportCollectionList(CollectionList collection)
        {
            FolderPickerResult? folderResult = await FolderPicker.Default.PickAsync();
            if (folderResult == null || folderResult.Folder == null)
                return string.Empty;
            string exportFolder = Path.Combine(folderResult.Folder.Path, collection.Name);
            string exportImagesFolder = Path.Combine(exportFolder, "Images");
            Directory.CreateDirectory(exportFolder);
            Directory.CreateDirectory(exportImagesFolder);

            string exportCollectionFilePath = Path.Combine(exportFolder, $"{collection.Name}.txt");
            using StreamWriter writer = new(exportCollectionFilePath);
            writer.WriteLine(collection.Name);
            foreach (Item item in collection.Items)
            {
                writer.WriteLine("[ITEM]");
                writer.WriteLine(item.Name);
                writer.WriteLine(item.Condition);
                writer.WriteLine(item.Price);
                writer.WriteLine(item.Rating);
                writer.WriteLine(item.Comment);
                writer.WriteLine(!string.IsNullOrEmpty(item.ImagePath) ? Path.Combine("Images", Path.GetFileName(item.ImagePath)) : "");
            }
            foreach (Item item in collection.Items)
            {
                if (!string.IsNullOrWhiteSpace(item.ImagePath) && File.Exists(item.ImagePath))
                {
                    string exportImagePath = Path.Combine(exportImagesFolder, Path.GetFileName(item.ImagePath));
                    if (!File.Exists(exportImagePath))
                        File.Copy(item.ImagePath, exportImagePath, overwrite: true);
                }
            }

            return exportFolder;
        }

        public static string ImportCollectionListFromFolder(string folderPath)
        {
            string[] txtFiles = Directory.GetFiles(folderPath, "*.txt");
            if (txtFiles.Length == 0)
                return "No collection list file (.txt) found in picked folder!";
            string collectionListPath = txtFiles[0];

            string exportedImagesFolder = Path.Combine(folderPath, "Images");
            string appImagesFolder = Path.Combine(FileSystem.AppDataDirectory, "Images");
            if (!Directory.Exists(appImagesFolder))
                Directory.CreateDirectory(appImagesFolder);
            if (Directory.Exists(exportedImagesFolder))
            {
                foreach (string exportedImagePath in Directory.GetFiles(exportedImagesFolder))
                {
                    string appImagesImagePath = Path.Combine(appImagesFolder, Path.GetFileName(exportedImagePath));
                    File.Copy(exportedImagePath, appImagesImagePath, overwrite: true);
                }
            }

            CollectionList? importedCollectionList = LoadCollectionListFromFile(collectionListPath);
            if (importedCollectionList == null) return "Collection list file is corrupted";
            foreach (Item item in importedCollectionList.Items)
            {
                if (!string.IsNullOrWhiteSpace(item.ImagePath))
                {
                    string imageName = Path.GetFileName(item.ImagePath);
                    item.ImagePath = Path.Combine(appImagesFolder, imageName);
                }
            }
            CollectionList? existingCollectionList = CollectionLists.FirstOrDefault(c => c.Name == importedCollectionList.Name);
            if (existingCollectionList != null)
            {
                foreach (Item importedItem in importedCollectionList.Items)
                {
                    if (!existingCollectionList.Items.Any(i => i.Name == importedItem.Name))
                        existingCollectionList.Items.Add(importedItem);
                }
            }
            else
            {
                CollectionLists.Add(importedCollectionList);
            }

            SaveCollectionListToFile(existingCollectionList ?? importedCollectionList);
            return String.Empty;
        }
    }
}
