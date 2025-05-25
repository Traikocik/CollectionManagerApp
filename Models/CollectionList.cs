using System;
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

        //public int Total => Items.Count;
        //public int Sold => Items.Count(i => i.Condition == "sold");
        //public int ForSale => Items.Count(i => i.Condition == "for sale");
    }
}
