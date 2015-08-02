using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KindleBookSummaryCreator
{
    public partial class MainForm : Form
    {
        const int MaxFilePathLength = 260;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DetectClippingsFile();

            // Set the desktop as the default output folder
            txtOutputFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Summaries";
        }

        private void DetectClippingsFile()
        {
            // Automatically select the clipping file
            var clippingFile = GetKindleClippingFile();

            if (clippingFile != null)
            {
                txtKindleFolder.Text = clippingFile;
            }
        }

        private string GetKindleClippingFile()
        {
            var drives = DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.Removable);

            foreach (var drive in drives)
            {
                string clippingPath = Path.Combine(drive.RootDirectory.FullName, @"documents\My Clippings.txt");

                if (File.Exists(clippingPath))
                {
                    return clippingPath;
                }
            }

            return null;
        }

        private void btnDetect_Click(object sender, EventArgs e)
        {
            DetectClippingsFile();
        }

        private void btnOutputFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtOutputFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                var streamReader = File.OpenText(txtKindleFolder.Text);

                var summaries = new Dictionary<string, string>();

                while (!streamReader.EndOfStream)
                {
                    var bookTitle = streamReader.ReadLine();
                    var type = streamReader.ReadLine();

                    if (type.Contains("Highlight"))
                    {
                        if (!summaries.ContainsKey(bookTitle))
                            summaries[bookTitle] = "";
                    }

                    var line = streamReader.ReadLine();
                    var text = "";
                    while (line != "==========")
                    {
                        text += line;
                        line = streamReader.ReadLine();
                    }

                    if (type.Contains("Highlight"))
                    {
                        summaries[bookTitle] += text + "\n";
                    }
                }

                var outputPath = txtOutputFolder.Text;

                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                foreach (var summary in summaries)
                {
                    var extension = ".txt";
                    var maxLenght = MaxFilePathLength - extension.Length;

                    var filePath = Path.Combine(outputPath, Sanitize(summary.Key) + extension);
                    filePath = filePath.Substring(0, Math.Min(filePath.Length, maxLenght));
                    
                    File.WriteAllText(filePath, summary.Value);
                }

                Process.Start(outputPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occured. Try again.\n\n" + ex.Message, "Kindle Book Summary Creator", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string Sanitize(string name)
        {
            var sb = new StringBuilder();

            foreach (var c in name.Where(c => !Path.GetInvalidFileNameChars().Contains(c)))
            {
                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
