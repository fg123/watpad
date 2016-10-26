using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Notepad.Model;
using System.IO;
using System.Globalization;

namespace Notepad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string autoSavePath = "";
        string initialDirectory = "C:\\Watpad";
        bool fileOpened = false;
        bool programChange = false;
        string oldText = "";
        public MainWindow()
        {
            InitializeComponent();
            this.Title = "Watpad - No File Opened";
            Random rnd = new Random();
            int imageIndex = rnd.Next(1, 14);
            BitmapImage image = new BitmapImage(new Uri("pack://application:,,,/Resources/background" + imageIndex + ".jpg"));
            BackgroundImage.ImageSource = image;
            UpdateTree();
            UpdateTextBoxStatus();
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock t = ((TextBlock)sender);
            string path = t.ToolTip.ToString();
            autoSavePath = path;
            if (!IsBinary(System.IO.File.ReadAllBytes(@path)))
            {
                programChange = true;
                string text = System.IO.File.ReadAllText(@path);
                
                this.Title = "Watpad - " + t.Text;
                fileOpened = true;
                UpdateTextBoxStatus();
                try
                { 
                    TextArea.Text = text.Replace("--", "•");
                    oldText = TextArea.Text;
                }
                finally
                { 
                    programChange = false;
                }
            }
        }
        bool IsBinary(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
                if (bytes[i] > 127)
                    return true;
            return false;
        }

        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            string path;
            if (DirectoryTree.SelectedItem != null)
            {
                path = ((Item)DirectoryTree.SelectedItem).Path;
            }
            else
            {
                path = initialDirectory;
            }
            FileAttributes a = File.GetAttributes(path);
            if (!a.HasFlag(FileAttributes.Directory))
            {
                path = Directory.GetParent(path).FullName;
            }
            string name = "\\Untitled.txt";
            int untitled = 0;
            while(File.Exists(path + name))
            {
                untitled++;
                name = "\\Untitled" + untitled + ".txt";
            }
            File.Create(path + name).Close();
            autoSavePath = path + name;
            TextArea.Text = "Untitled" + (untitled != 0 ? untitled.ToString() : "");
            oldText = TextArea.Text;
            this.Title = "Watpad - " + TextArea.Text;
            TextArea.Focus();
            UpdateTree();
            fileOpened = true;
            UpdateTextBoxStatus();
        }
        private void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            string path;
            if (DirectoryTree.SelectedItem != null)
            {
                path = ((Item)DirectoryTree.SelectedItem).Path;
            }
            else
            {
                path = initialDirectory;
            }
            FileAttributes a = File.GetAttributes(path);
            if (!a.HasFlag(FileAttributes.Directory))
            {
                string sMessageBoxText = "Are you sure you want to delete?";
                string sCaption = "Watpad";

                MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
                MessageBoxImage icnMessageBox = MessageBoxImage.None;

                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

                switch (rsltMessageBox)
                {
                    case MessageBoxResult.Yes:
                        File.Delete(path);
                        autoSavePath = "";
                        TextArea.Text = "";
                        UpdateTree();
                        fileOpened = false;
                        UpdateTextBoxStatus();
                        break;

                    case MessageBoxResult.No:
                        /* ... */
                        break;
                }
            }
        }

        private void TextArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            int x = TextArea.SelectionStart;
            TextArea.Text = TextArea.Text.Replace("--", "•");
            TextArea.SelectionStart = x;
            
            if(!programChange)
            {
                
                if(TextArea.Text.Contains(Environment.NewLine) && fileOpened)
                {
                    string title = TextArea.Text.Substring(0, TextArea.Text.IndexOf(Environment.NewLine));
                    string oldTitle = oldText.Contains(Environment.NewLine)? oldText.Substring(0, oldText.IndexOf(Environment.NewLine)) : oldText;
                    if(title != oldTitle)
                    { 
                        if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(autoSavePath))
                        {
                            FileAttributes a = File.GetAttributes(autoSavePath);
                            string dir = Directory.GetParent(autoSavePath).FullName;
                            if (!File.Exists(dir + "\\" + title + ".txt"))
                            { 
                                File.Move(@autoSavePath, (dir + "\\" + title + ".txt"));
                                autoSavePath = (dir + "\\" + title + ".txt");
                                this.Title = "Watpad - " + title + ".txt";
                                UpdateTree();
                            }
                        }
                    }
                }
                if (autoSavePath != "")
                {
                    try
                    {
                        System.IO.File.WriteAllText(@autoSavePath, TextArea.Text.Replace("•", "--"));
                    }
                    catch (System.IO.IOException)
                    {
                        Console.Write("Error");
                    }
                }
            }
        }
        void UpdateTree()
        {
            List<string> nodeStates = new List<string>();
            nodeStates = RetrieveExpanded(DirectoryTree);
            var itemProvider = new ItemProvider();
            var items = itemProvider.GetItems(initialDirectory);
            DataContext = items;
            RestoreExpanded(DirectoryTree, nodeStates);
        }
        void RestoreExpanded(ItemsControl parent, List<string> states)
        {
            foreach (Object item in parent.Items)
            {
                TreeViewItem currentContainer = (TreeViewItem)(parent.ItemContainerGenerator.ContainerFromItem(item));
                Item theItem = item as Item;
                if (states.Contains(theItem.Path))
                {
                    currentContainer.IsExpanded = true;
                }
            }
        }
        List<string> RetrieveExpanded(ItemsControl parentContainer)
        {
            List<string> nodes = new List<string>();
            foreach (Object item in parentContainer.Items)
            {
                TreeViewItem currentContainer = (TreeViewItem)(parentContainer.ItemContainerGenerator.ContainerFromItem(item));
                Item theItem = item as Item;
                if (currentContainer != null && currentContainer.Items.Count > 0)
                {
                    // Expand the current item. 
                    if (currentContainer.IsExpanded)
                    {
                        nodes.Add(theItem.Path);
                    }
                    // If the sub containers of current item is ready, we can directly go to the next 
                    // iteration to expand them. 
                    nodes.AddRange(RetrieveExpanded(currentContainer));
                }
            }
            return nodes;
        } 
        void UpdateTextBoxStatus()
        {
            if (fileOpened)
            {
                TextArea.IsEnabled = true;
            }
            else
            {
                TextArea.IsEnabled = false;
                this.Title = "Watpad - No File Opened";
                TextArea.Text = "Welcome to Watpad - By Felix Guo"
                    + Environment.NewLine + "Make a new file or open a file!";
            }
        }

        private void TextArea_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                Random rnd = new Random();
                int imageIndex = rnd.Next(1, 14);
                BitmapImage image = new BitmapImage(new Uri("pack://application:,,,/Resources/background" + imageIndex + ".jpg"));
                BackgroundImage.ImageSource = image;
            }
            else if (e.Key == Key.F3)
            {
                TextArea.FontSize += 2;
            }
            else if (e.Key == Key.F2)
            {
                TextArea.FontSize -= 2;
            }
        }
    }
    public class ItemProvider
    {
        public List<Item> GetItems(string path)
        {
            var items = new List<Item>();

            var dirInfo = new DirectoryInfo(path);

            foreach (var directory in dirInfo.GetDirectories())
            {
                var item = new DirectoryItem
                {
                    Name = directory.Name,
                    Path = directory.FullName,
                    Items = GetItems(directory.FullName)
                };

                items.Add(item);
            }

            foreach (var file in dirInfo.GetFiles())
            {
                var item = new FileItem
                {
                    Name = file.Name,
                    Path = file.FullName
                };

                items.Add(item);
            }

            return items;
        }
    }

}
