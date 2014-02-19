using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Resources;
using System.IO;

namespace FileBrower
{
    public partial class FileBrower : Form
    {
        string path = "E:/";
        Stack<string> paths = new Stack<string>();

        ResourceManager rm = new ResourceManager("FileBrower.Resource", typeof(FileBrower).Assembly);

        public FileBrower()
        {    
            InitializeComponent();
            LoadFiles();
        }

        private void LoadFiles()
        {
    
            List<string> subDirectories = new List<string>(Directory.EnumerateDirectories(path));
            List<string> subFiles = new List<string>(Directory.EnumerateFiles(path));
            List<ItemData> all = new List<ItemData>();
            foreach (string directoryPath in subDirectories)
            {
                ItemData item = new ItemData();
                item.Name = Path.GetFileName(directoryPath);
                item.Icon = (Image)rm.GetObject("directory");
                all.Add(item);
            }
            foreach (string filePath in subFiles)
            {
                ItemData item = new ItemData();
                item.Name = Path.GetFileName(filePath);
                item.Icon = (Image)rm.GetObject("file");
                all.Add(item);
            }
            if (paths.Count > 0)
            {
                ItemData item = new ItemData();
                item.Name = "UP";
                item.Icon = (Image)rm.GetObject("up");
                all.Insert(0, item);
            }
            listBox1.Items.Clear();
            listBox1.Items.AddRange(all.ToArray());
        }


        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {

            e.DrawBackground();
            // Draw the current item text based on the current Font 
            // and the custom brush settings.
            ItemData data = (ItemData)listBox1.Items[e.Index];
            String name = data.Name;
            Image image = data.Icon;

            int leftMargin = 10;
            e.Graphics.DrawImage(image,new Point(leftMargin,e.Bounds.Y + (e.Bounds.Height - image.Height)/2));
            SizeF hT = TextRenderer.MeasureText(name, e.Font);
            e.Graphics.DrawString(name,e.Font,Brushes.Black,new PointF(leftMargin + image.Width + 5, e.Bounds.Y + (e.Bounds.Height - hT.Height)/2));
            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
            
        }

        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
            int index = this.listBox1.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                Console.WriteLine(index);
                String name = ((ItemData)listBox1.Items[index]).Name;
                String filePath = path + "\\" + name;
                if (index == 0 && paths.Count > 0)
                {
                    string sname = paths.Pop();
                    path = path.Substring(0,path.LastIndexOf("\\"));
                    LoadFiles();
                }
                else if ((File.GetAttributes(filePath) & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    Console.WriteLine("open dir " + filePath);
                    paths.Push(name);
                    string tpath = path;
                    path = filePath;
                    try
                    {
                        LoadFiles();
                    }
                    catch (UnauthorizedAccessException uae)
                    {
                        path = tpath;
                        paths.Pop();
                        Console.WriteLine(uae.ToString());
                        MessageBox.Show("Unauthorized Access");
                    }
                }
                else
                {
                    System.Diagnostics.Process.Start(filePath);
                }
            }
        }

        private void OpenSelectItem()
        {
 
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Back:
                    break;
                case Keys.Enter:
                    if (listBox1.SelectedIndex != -1)
                    {
                        string name = ((ItemData)listBox1.SelectedItem).Name;
                        string filepath = path + "\\" + name;
                        Console.WriteLine("press enter then open dir " + filepath);
                        paths.Push(name);
                        string tpath = path;
                        path = filepath;
                        try
                        {
                            LoadFiles();
                        }
                        catch (UnauthorizedAccessException uae)
                        {
                            path = tpath;
                            paths.Pop();
                            Console.WriteLine(uae.ToString());
                            MessageBox.Show("Unauthorized Access");
                        }
 
                    }
                   
                    break;

            }
        }   
    }

    class ItemData 
    {
         string name;
         Image icon;
        public string Name
        {
            set { name = value; }
            get { return name; }
        }

        public Image Icon
        {
            set { icon = value; }
            get {return icon;}
        }
    }
}
