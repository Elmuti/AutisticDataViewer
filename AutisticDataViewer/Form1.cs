using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Windows;
using System.Runtime.Serialization;
using System.IO;

namespace AutisticDataViewer
{
    public partial class Form1 : Form
    {
        private Dictionary<string, List<AutisticData>> CurrentData = new Dictionary<string, List<AutisticData>>();
        private TreeNode SelectedNode;
        private int LatestFolderId = 1;
        private int LatestEntryId = 1;
        private bool CtrlDown = false;
        private AutisticData CurrentCopy;

        private void KeyDown_Global(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                CtrlDown = true;
            }
            //MessageBox.Show("ctrldown: " + CtrlDown.ToString() + ", key: " + e.KeyCode.ToString());
            if (CtrlDown && e.KeyCode == Keys.C)
            {
                CopyEntry();
            }
            else if (CtrlDown && e.KeyCode == Keys.V)
            {
                PasteEntry();
            }
        }

        private void KeyUp_Global(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            //MessageBox.Show(e.KeyCode.ToString());
            if (e.KeyCode == Keys.ControlKey)
            {
                CtrlDown = false;
            }
        }

        private void CopyEntry()
        {
            //MessageBox.Show("copy");
            if (SelectedNode.Level == 1)
            { 
                foreach (KeyValuePair<string, List<AutisticData>> pair in CurrentData)
                {
                    foreach (AutisticData d in pair.Value)
                    {
                        if (d.EntryName == SelectedNode.Text)
                        {
                            CurrentCopy = d;
                            break;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Cannot copy folders!");
            }
        }

        private void PasteEntry()
        {
            if (SelectedNode.Level == 0)
            {
                if (CurrentCopy != null)
                {
                    CurrentData[SelectedNode.Text].Add(CurrentCopy);
                    SetTreeFromData();
                }
                else
                {
                    MessageBox.Show("No entry to paste!");
                }
            }
            else
            {
                MessageBox.Show("No folder selected to paste entry into!");
            }
        }

        private void CreateNew()
        {
            if (MessageBox.Show("Are you sure? You will lose current progress.", "Creating a new file", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                CurrentData = new Dictionary<string, List<AutisticData>>();
                richTextBox1.Text = ""; //entry name
                richTextBox2.Text = ""; //source
                richTextBox5.Text = ""; //authors
                richTextBox4.Text = ""; //keywords
                richTextBox7.Text = ""; //real name
                richTextBox6.Text = ""; //date added
                richTextBox3.Text = ""; //notes
                treeView1.Nodes.Clear();
                this.Text = "Autistic Data Viewer";
                LatestEntryId = 1;
                LatestFolderId = 1;
                SelectedNode = null;
            }
        }

        private void Save()
        {
            saveFileDialog1.Filter = "Binary File|*.bin";
            saveFileDialog1.Title = "Save File";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    //save text to dictionary entry
                    if (SelectedNode.Level == 1)
                    {
                        foreach (KeyValuePair<string, List<AutisticData>> pair in CurrentData)
                        {
                            foreach (AutisticData d in pair.Value)
                            {
                                if (d.EntryName == SelectedNode.Text)
                                {
                                    d.EntryName = richTextBox1.Text;
                                    d.Source = richTextBox2.Text;
                                    d.Author = richTextBox5.Text;
                                    d.Keywords = richTextBox4.Text;
                                    d.RealName = richTextBox7.Text;
                                    d.DateAdded = richTextBox6.Text;
                                    d.Notes = richTextBox3.Text;
                                    Console.WriteLine("Saved textboxes to dictionary");
                                }
                            }
                        }
                    }

                    var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                    var fi = new FileInfo(saveFileDialog1.FileName);

                    using (var binaryFile = fi.Create())
                    {
                        binaryFormatter.Serialize(binaryFile, CurrentData);
                        binaryFile.Flush();
                    }
                }
            }
        }

        private void Open()
        {
            openFileDialog1.Filter = "Binary File|*.bin";
            openFileDialog1.Title = "Select a File";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (openFileDialog1.FileName != "" && File.Exists(openFileDialog1.FileName))
                {
                    richTextBox1.Text = ""; //entry name
                    richTextBox2.Text = ""; //source
                    richTextBox5.Text = ""; //authors
                    richTextBox4.Text = ""; //keywords
                    richTextBox7.Text = ""; //real name
                    richTextBox6.Text = ""; //date added
                    richTextBox3.Text = ""; //notes
                    treeView1.Nodes.Clear();
                    LatestEntryId = 1;
                    LatestFolderId = 1;
                    SelectedNode = null;

                    var fi = new FileInfo(openFileDialog1.FileName);
                    var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    using (var binaryFile = fi.OpenRead())
                    {
                        CurrentData = (Dictionary<string, List<AutisticData>>)binaryFormatter.Deserialize(binaryFile);
                        this.Text = "Autistic Data Viewer - " + openFileDialog1.FileName;
                    }

                    SetTreeFromData();
                    treeView1.ExpandAll();
                }
            }
        }

        private void Print_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Text Files|*.txt";
            saveFileDialog1.Title = "Save File";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    DataParser.PrintToFile(saveFileDialog1.FileName, CurrentData);
                }
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void RenameFld_Click(object sender, EventArgs e)
        {
            if (SelectedNode.Level == 0)
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox("Folder Name:", "Rename Folder", "New Folder " + LatestFolderId.ToString(), -1, -1);
                CurrentData.Add(input, CurrentData[SelectedNode.Text]);
                CurrentData.Remove(SelectedNode.Text);
                SelectedNode.Text = input;
            }
        }

        private void RenameEntry_Click(object sender, EventArgs e)
        {
            if (SelectedNode.Level == 1)
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox("Entry Name:", "Rename Entry", "New Entry " + LatestEntryId.ToString(), -1, -1);
                GetEntry(SelectedNode.Parent.Text, SelectedNode.Index).EntryName = input;
                richTextBox1.Text = input;
                SelectedNode.Text = input;
            }
        }

        private void KeyDown_Press(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (SelectedNode.Level == 0) // Folder
                {
                    foreach (KeyValuePair<string, List<AutisticData>> pair in CurrentData)
                    {
                        if (pair.Key == SelectedNode.Text)
                        {
                            CurrentData.Remove(pair.Key);
                            break;
                        }
                    }
                    SelectedNode.Remove();
                }
                else  // Entry
                {
                    foreach (KeyValuePair<string, List<AutisticData>> pair in CurrentData)
                    {
                        foreach (AutisticData d in pair.Value)
                        {
                            if (d.EntryName == SelectedNode.Text)
                            {
                                pair.Value.Remove(d);
                                break;
                            }
                        }
                    }
                    SelectedNode.Remove();
                }
            }
        }

        private void SetTreeFromData()
        {
            treeView1.Nodes.Clear();
            foreach (KeyValuePair<string, List<AutisticData>> pair in CurrentData)
            {
                TreeNode fld = treeView1.Nodes.Add(pair.Key);
                foreach (AutisticData d in pair.Value)
                {
                    fld.Nodes.Add(d.EntryName);
                }
            }
            treeView1.ExpandAll();
        }


        private void Open_Click(object sender, EventArgs e)
        {
            Open();
        }

        private void New_Click(object sender, EventArgs e)
        {
            CreateNew();
        }

        private List<AutisticData> GetFolderFromName(string folderName)
        {
            foreach(KeyValuePair<string, List<AutisticData>> pair in CurrentData)
            {
                if (pair.Key == folderName)
                    return pair.Value;
            }
            return new List<AutisticData>();
        }

        private AutisticData GetEntryFromName(string entryName)
        {
            foreach (KeyValuePair<string, List<AutisticData>> pair in CurrentData)
            {
                foreach(AutisticData d in pair.Value)
                {
                    if (d.EntryName == entryName)
                        return d;
                }
                    
            }
            return new AutisticData();
        }

        private AutisticData GetEntry(string folder, int entry)
        {
            List<AutisticData> entries = CurrentData[folder];

            int c = 0;
            foreach(AutisticData d in entries)
            {
                if (c == entry)
                    return d;

                c++;
            }
            return new AutisticData();
        }


        protected void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (SelectedNode == null)
            {
                SelectedNode = e.Node;
            }
            else
            {
                //save text to dictionary entry
                if (SelectedNode.Level == 1 && SelectedNode != e.Node)
                {
                    foreach (KeyValuePair<string, List<AutisticData>> pair in CurrentData)
                    {
                        foreach (AutisticData d in pair.Value)
                        {
                            if (d.EntryName == SelectedNode.Text)
                            {
                                d.EntryName = richTextBox1.Text;
                                d.Source = richTextBox2.Text;
                                d.Author = richTextBox5.Text;
                                d.Keywords = richTextBox4.Text;
                                d.RealName = richTextBox7.Text;
                                d.DateAdded = richTextBox6.Text;
                                d.Notes = richTextBox3.Text;
                                Console.WriteLine("Saved textboxes to dictionary");
                            }
                        }
                    }
                }
            }

            SelectedNode = e.Node;

            //load text from dictionary
            if (e.Node.Level == 1)
            {
                button3.Enabled = false;
                button4.Enabled = true;
                //AutisticData data = GetEntryFromName(e.Node.Text);
                AutisticData data = GetEntry(SelectedNode.Parent.Text, e.Node.Index);
                richTextBox1.Text = data.EntryName; //entry name
                richTextBox2.Text = data.Source; //source
                richTextBox5.Text = data.Author; //authors
                richTextBox4.Text = data.Keywords; //keywords
                richTextBox7.Text = data.RealName; //real name
                richTextBox6.Text = data.DateAdded; //date added
                richTextBox3.Text = data.Notes; //notes
            }
            else
            {
                button3.Enabled = true;
                button4.Enabled = false;
                richTextBox1.Text = ""; //entry name
                richTextBox2.Text = ""; //source
                richTextBox5.Text = ""; //authors
                richTextBox4.Text = ""; //keywords
                richTextBox7.Text = ""; //real name
                richTextBox6.Text = ""; //date added
                richTextBox3.Text = ""; //notes
            }
            treeView1.ExpandAll();
        }

        private void AddFolder(object sender, EventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Folder Name:", "Add Folder", "New Folder " + LatestFolderId.ToString(), -1, -1);
            if (input == "")
                return;

            treeView1.Nodes.Add(input);

            CurrentData.Add(input, new List<AutisticData>());
            treeView1.ExpandAll();
            LatestFolderId++;
        }

        private void AddEntry(object sender, EventArgs e)
        {

            if (SelectedNode.Level == 0)
            {
                string str = SelectedNode.Text;
                string input = Microsoft.VisualBasic.Interaction.InputBox("Entry Name:", "Add Entry", "New Entry " + LatestEntryId.ToString(), -1, -1);

                if (input == "")
                    return;

                treeView1.SelectedNode.Nodes.Add(input);

                CurrentData[str].Add(new AutisticData(input));
            }
            else
            {
                MessageBox.Show("No folder selected!");
            }
            treeView1.ExpandAll();
            LatestEntryId++;
        }

        private void EntryName_Changed(object sender, EventArgs e)
        {
            if (SelectedNode.Level == 1)
            {
                if (richTextBox1.Text != "")
                    SelectedNode.Text = richTextBox1.Text;
            }
            treeView1.ExpandAll();
        }

        public Form1()
        {
            InitializeComponent();
            
        }


        private int GetKeyIndex(string key)
        {
            int i = 0;
            foreach(KeyValuePair<string, List<AutisticData>> pair in CurrentData)
            {
                if (pair.Key == key)
                    return i;

                i++;
            }
            return 0;
        }
    }
}
