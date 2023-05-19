using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Pkcs;
using Microsoft.VisualBasic;
using NAudio.Mixer;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.IO;
using System.Drawing.Printing;
using System.Text.Json;
using System.CodeDom;

namespace Beeper
{
    public partial class Form1 : Form
    {
        readonly string settingsFile = "settings.json";
        Settings settings;
        BindingList<FileProcessor> items = new BindingList<FileProcessor>();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSettings();
            listBox1.DataSource = items;
            this.Text = $"Video beeper {AppVersion.TAG}";
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (var file in ofd.FileNames)
                {
                    ProcessFile(file);
                }
            }
        }

        void ProcessFile(string path)
        {
            FileProcessor fileProcessor = new FileProcessor(this);
            string folder = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);
            string extention = Path.GetExtension(path);
            fileProcessor.Start(settings, path, Path.Combine(folder, filename + "_out.mp4"));
            items.Add(fileProcessor);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

        void LoadSettings()
        {
            if(!File.Exists(settingsFile))
                settings = new Settings();
            else
                settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(settingsFile)) ?? new Settings();
            propertyGrid1.SelectedObject = settings;
        }
        void SaveSettings()
        {
            string path = Path.GetDirectoryName(settingsFile);
            if(!string.IsNullOrEmpty(path))
                Directory.CreateDirectory(path);
            File.WriteAllText(settingsFile, JsonSerializer.Serialize(settings));
        }

    }
}
