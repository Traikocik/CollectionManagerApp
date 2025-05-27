using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionManagerApp.Models
{
    public class Item
    {
        public string Name { get; set; }
        public string Condition { get; set; }
        public decimal Price { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public string ImagePath { get; set; }
    }
}
