using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Notepad.Model;

namespace Notepad.Model
{
    public class Item
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
    public class FileItem : Item
    {

    }
    public class DirectoryItem : Item
    {
        public List<Item> Items { get; set; }

        public DirectoryItem()
        {
            Items = new List<Item>();
        }
    }
}