﻿using System;
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
        FileSystemWatcher watcher = new FileSystemWatcher();
        Stack<string> paths = new Stack<string>();

        ResourceManager rm = new ResourceManager("FileBrower.Resource", typeof(FileBrower).Assembly);

        public FileBrower()
        {    
            InitializeComponent();
            initWatcher();
            LoadFiles();
        }

        private void initWatcher()
        {
            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
            watcher.Path = path;
            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        private void LoadFiles()
        {

            string[] extensions = { ".pdf", ".doc" ,".txt"};
            List<string> subDirectories = new List<string>(Directory.EnumerateDirectories(path));
            List<string> subFiles = new List<string>(Directory.EnumerateFiles(path).Where(s => extensions.Any(ext => ext == Path.GetExtension(s))));
            List<ItemData> all = new List<ItemData>();
            foreach (string directoryPath in subDirectories)
            {
                ItemData item = new ItemData();
                item.Name = Path.GetFileName(directoryPath);
                item.Direcory = true;
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
                item.Direcory = true;
                all.Insert(0, item);
            }
            if (listBox1.InvokeRequired)
            {
                Action a = () => {
                    listBox1.Items.Clear();
                    listBox1.Items.AddRange(all.ToArray());
                };
                listBox1.Invoke(a);

            }
            else
            {
                listBox1.Items.Clear();
                listBox1.Items.AddRange(all.ToArray());
            }
            watcher.Path = path;
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
            int rightMargin =10;
            e.Graphics.DrawImage(image,new Point(leftMargin,e.Bounds.Y + (e.Bounds.Height - image.Height)/2));
            SizeF hT = TextRenderer.MeasureText(name, e.Font);
            e.Graphics.DrawString(name,e.Font,Brushes.Black,new PointF(leftMargin + image.Width + 5, e.Bounds.Y + (e.Bounds.Height - hT.Height)/2));
            if (!data.Direcory)
            {
                Pen skyBluePen = new Pen(Brushes.DeepSkyBlue);
                int buttonWidth = 50;
                int buttonHeight = 30;

                Rectangle deleteButtonBounds = new Rectangle(e.Bounds.Width - buttonWidth - rightMargin, e.Bounds.Y + (e.Bounds.Height - buttonHeight) / 2, buttonWidth, buttonHeight);
                if (deleteButtonBounds.Contains(mouseDownPoint))
                {
                    e.Graphics.FillRectangle(Brushes.Chocolate, deleteButtonBounds);
                    deleteClicked = true;
                }
                else
                {
                    e.Graphics.DrawRectangle(new Pen(Brushes.Chocolate), deleteButtonBounds);
                }

                string deleteString = rm.GetString("delete");
                hT = TextRenderer.MeasureText(deleteString, e.Font);
                e.Graphics.DrawString(deleteString, e.Font, Brushes.Black, new PointF(e.Bounds.Width - buttonWidth - rightMargin + (buttonWidth - hT.Width) / 2, e.Bounds.Y + (e.Bounds.Height - hT.Height) / 2));
            }
        }

        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
            Console.WriteLine(mouseDownPoint);

            int index = this.listBox1.IndexFromPoint(e.Location);
            if (index >= 0)
            {
                Console.WriteLine(index);

            }
        }

        private void UP()
        {
            if (paths.Count > 0)
            {
                string sname = paths.Pop();
                path = path.Substring(0, path.LastIndexOf("\\"));
                LoadFiles();
            }
        }

        private void OpenSelectItem()
        {
            if (listBox1.SelectedIndex != -1)
            {
                if (listBox1.SelectedIndex == 0 && paths.Count > 0)
                {
                    UP();
                }
                else
                {
                    string name = ((ItemData)listBox1.SelectedItem).Name;
                    string filePath = path + "\\" + name;
                    if ((File.GetAttributes(filePath) & FileAttributes.Directory) == FileAttributes.Directory)
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
 
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Back:
                    UP();
                    break;
                case Keys.Enter:
                    OpenSelectItem();
                    break;

            }
        }

        private Point mouseDownPoint = Point.Empty;
        private bool deleteClicked = false;
        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDownPoint = e.Location;
            int index = listBox1.IndexFromPoint(e.Location);
            if (index >= 0)
            {
                listBox1.Invalidate(listBox1.GetItemRectangle(index));
            }
            
        }

        private void listBox1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDownPoint = Point.Empty;
            int index = listBox1.IndexFromPoint(e.Location);
            if (index >= 0)
            {
                listBox1.Invalidate(listBox1.GetItemRectangle(index));
                if (!deleteClicked)
                {
                    OpenSelectItem();
                }
                else
                {
                    DialogResult dialogResult = MessageBox.Show("Sure", "Some Title", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        String name = ((ItemData)listBox1.Items[index]).Name;
                        String filePath = path + "\\" + name;
                        try
                        {
                            File.Delete(filePath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            MessageBox.Show(ex.ToString());
                        }
                        finally
                        {
                            LoadFiles();
                        }
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        //do something else
                    }
                   
                }
                deleteClicked = false;
            }
        }   

        // Define the event handlers.
        private  void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            LoadFiles();
        }

        private  void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
            LoadFiles();
        }

    }

    class ItemData 
    {
         string name;
         Image icon;
         bool direcory;

         public bool Direcory
         {
             get { return direcory; }
             set { direcory = value; }
         }
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
