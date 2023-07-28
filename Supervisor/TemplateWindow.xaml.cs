using Microsoft.Win32;
using Supervisor.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Supervisor
{
    /// <summary>
    /// Logica di interazione per TemplateWindow.xaml
    /// </summary>
    public partial class TemplateWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Selected { get; set; }
        public string SelectedTemplate { get; set; }
        public string NewFile
        {
            get { return _NewFile; }
            set { _NewFile = value; OnPropertyChanged(); }
        }

        private string _NewFile;
        private List<string> templates;
        private FileInfo[] templateFiles;

        public TemplateWindow()
        {
            InitializeComponent();

            templates = new List<string>();

            DataContext = this;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (Selected < 0) return;
            if (string.IsNullOrEmpty(NewFile)) return;

            SelectedTemplate = templateFiles[Selected].FullName;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PathButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "XML file | *.xml";
            sfd.DefaultExt = ".xml";
            if (new DirectoryInfo(Settings.Default.NewFilePath).Exists)
                sfd.InitialDirectory = Settings.Default.NewFilePath;
            if (sfd.ShowDialog() == true)
            {
                Settings.Default.NewFilePath = Path.GetDirectoryName(sfd.FileName);
                Settings.Default.Save();
                NewFile = sfd.FileName;
            }
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            string dbPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\templates";
            DirectoryInfo db = Directory.CreateDirectory(dbPath);
            templateFiles = db.GetFiles();
            for (int i = 0; i < templateFiles.Length; i++)
            {
                if (templateFiles[i].Extension == ".xml")
                    templates.Add(Path.GetFileNameWithoutExtension(templateFiles[i].Name));
            }
            TemplatesListview.ItemsSource = templates;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
    }
}
