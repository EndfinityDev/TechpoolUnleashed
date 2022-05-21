using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TechpoolUnleashed.SQLite;
using TechpoolUnleashed.Windows;

namespace TechpoolUnleashed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;

        public const UInt32 VERSION = 20220521;

        public string DBFilePath = "";
        public DatabaseManager DBManager;
        public byte[] ReturnTechpool;
        public string ReturnUID;
        public string ReturnName;
        public LoadedItemType ItemType;
        
        public readonly double ParamTemplateHeight;

        Grid _topLevelGrid;
        Grid _loadButtonGrid;
        Grid _techPoolGrid;
        Grid _utilityGrid;
        Grid _techpoolGrid;
        TechpoolParameterTemplate _parameterEntryTemplate;

        Button _modelTechpoolButton;
        Button _trimTechpoolButton;
        Button _familyTechpoolButton;
        Button _variantTechpoolButton;

        Button _settingsButton;
        Button _applyChangesButton;
        Button _resetChangesButton;

        Label _selectedItemLabel;

        DeserializedItemTable _currentItemTable;

        Grid _parameterTemplate;

        double _maxTechPool = 15;
        double _minTechPool = 0;


        List<Grid> _loadedTechpoolGrids = new List<Grid>();
        static Dictionary<string, double> _originalParameters = new Dictionary<string, double>();
        static Dictionary<string, double> _newParameters = new Dictionary<string, double>();

        string _loadedItem;


        public MainWindow()
        {
            Instance = this;

            foreach (var proc in Process.GetProcesses())
            {
                if(proc.ProcessName == "AutomationGame-Win64-Shipping")
                    MessageBox.Show("Automation is open. Please close the game before applying any changes.",
                        "Automation is open",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
            }
            //_dbFilePath = "testdatabase.db";

            //if (File.Exists(Environment.GetCommandLineArgs()[0]))
            //    DBFilePath = Environment.GetCommandLineArgs()[0];

            //if (DBFilePath == "")
            ReadSettingsFile();

            //Console.WriteLine(DBFilePath);

            while (true)
            {
                if (DBFilePath == "")
                {
                    OpenFileDialog fileDialog = new OpenFileDialog();
                    fileDialog.InitialDirectory = "C:\\";
                    fileDialog.Filter = "Database files (*.db)|*db|All files (*.*)|*.*";
                    fileDialog.FilterIndex = 1;
                    fileDialog.RestoreDirectory = true;
                    fileDialog.Title = "Select save database";

                    if ((bool)fileDialog.ShowDialog())
                        DBFilePath = fileDialog.FileName;
                    else
                        Environment.Exit(0);
                }

                if (File.Exists(DBFilePath))
                {
                    try
                    {
                        DBManager = new DatabaseManager(DBFilePath);
                        break;
                    }
                    catch (SQLiteException e)
                    {
                        MessageBox.Show("Could not open database\n" + e.Message, "Database read error", MessageBoxButton.OK, MessageBoxImage.Error);
                        DBFilePath = "";
                    }
                }
                else
                    DBFilePath = "";
            }

            SaveSettingsFile();

            {
                Task checkVersionTask = new Task(CheckForUpdates);
                checkVersionTask.Start();
            }

            InitializeComponent();
            this.Title = "Techpool Unleashed: " + DBFilePath;
            RegisterTemplates();
            ParamTemplateHeight = _parameterTemplate.Height;
            _modelTechpoolButton.Click += LoadModelTechpoolTemplate;
            _trimTechpoolButton.Click += LoadTrimTechpoolTemplate;
            _familyTechpoolButton.Click += LoadFamilyTechpoolTemplate;
            _variantTechpoolButton.Click += LoadVariantTechpoolTemplate;

            _resetChangesButton.Click += ResetLoadedParameters;
            _applyChangesButton.Click += ApplyChanges;
            _settingsButton.Click += OpenSettings;

            this.Closing += MainWindow_Closing;
        }


        public void SaveSettingsFile()
        {
            if (!File.Exists("settings.settings"))
                File.Create("settings.settings").Close();

            List<string> settingsToWrite = new List<string>();
            settingsToWrite.Add("DataBase_Path " + DBFilePath);
            settingsToWrite.Add("Version " + VERSION);

            File.WriteAllLines("settings.settings", settingsToWrite.ToArray());
        }

        private void ReadSettingsFile()
        {
            if (File.Exists("settings.settings"))
            {
                var lines = File.ReadAllLines("settings.settings");

                foreach(var line in lines)
                {
                    var sline = line.Split(' ');
                    if (sline[0] == "DataBase_Path")
                    {
                        var rline = "";
                        for (Int32 i = 1; i < sline.Length; i++)
                        {
                            rline += sline[i];
                        }
                        //Console.WriteLine("Rline " + rline);
                        DBFilePath = rline;
                    }
                }
            }

            if(DBFilePath == "")
            {
                VersionSelectionWindow wnd = new VersionSelectionWindow(this);
                wnd.ShowDialog();
            }
        }
        void RegisterTemplates()
        {
            _topLevelGrid = (Grid)this.FindName("TopLevelGrid");
            _loadButtonGrid = (Grid)_topLevelGrid.FindName("LoadButtonGrid");
            _techPoolGrid = (Grid)_topLevelGrid.FindName("TechPoolGrid");
            _techpoolGrid = (Grid)_topLevelGrid.FindName("TechPoolStackPanel");
            _utilityGrid = (Grid)_topLevelGrid.FindName("UtilityGrid");

            _parameterTemplate = (Grid)_techpoolGrid.FindName("ParameterTemplate");

            _modelTechpoolButton = (Button)_topLevelGrid.FindName("ModelTechpoolButton");
            _trimTechpoolButton = (Button)_topLevelGrid.FindName("TrimTechpoolButton");
            _familyTechpoolButton = (Button)_topLevelGrid.FindName("FamilyTechpoolButton");
            _variantTechpoolButton = (Button)_topLevelGrid.FindName("VariantTechpoolButton");

            _selectedItemLabel = (Label)_loadButtonGrid.FindName("SelectionLabel");

            _settingsButton = (Button)_utilityGrid.FindName("SettingsButton");
            _applyChangesButton = (Button)_utilityGrid.FindName("ApplyButton");
            _resetChangesButton = (Button)_utilityGrid.FindName("ResetButton");

            _techpoolGrid.Children.Remove(_parameterTemplate);
            _topLevelGrid.Children.Add(_parameterTemplate);

            Slider slider = (Slider)_parameterTemplate.FindName("ParameterSlider");
            TextBox sliderLabel = (TextBox)_parameterTemplate.FindName("ParameterDisplay");
            Label paramLabel = (Label)_parameterTemplate.FindName("ParameterNameLabel");

            slider.Maximum = _maxTechPool;
            slider.Minimum = _minTechPool;

            _parameterEntryTemplate = new TechpoolParameterTemplate(_parameterTemplate, paramLabel, slider, sliderLabel);

            _applyChangesButton.IsEnabled = false;
            _resetChangesButton.IsEnabled = false;
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CheckUpdatedValues())
            {
                MessageBoxResult result = MessageBox.Show("Some settings have been unapplied.\nWould you like to apply them?",
                    "Warning!",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        ApplyChanges(this, null);
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                    default: 
                        break;
                }
            }
        }
        private void LoadFamilyTechpoolTemplate(object sender, RoutedEventArgs e)
        {
            FamilySelectionWindow wnd = new FamilySelectionWindow(this);
            //this.IsManipulationEnabled = false;
            wnd.ShowDialog();

            _loadedItem = this.ReturnName;
            if (ReturnTechpool == null || ReturnTechpool.Length == 0)
            {
                _currentItemTable = null;
                return;
            }
            _currentItemTable = new DeserializedItemTable(ReturnTechpool);

            //_loadedItem = "modeltechpool.bin";
            //_currentItemTable = new DeserializedItemTable(File.ReadAllBytes("familytechpool.bin")); // Replace reading files with reading
                                                                                              // from database
            CreateTechpoolMenu(_currentItemTable, sender, e);
            _selectedItemLabel.Content = (object)_loadedItem;
        }
        private void LoadVariantTechpoolTemplate(object sender, RoutedEventArgs e)
        {
            //_loadedItem = "varianttechpool.bin";
            //_currentItemTable = new DeserializedItemTable(File.ReadAllBytes("varianttechpool.bin"));
            VariantSelectionWindow wnd = new VariantSelectionWindow(this);
            //this.IsManipulationEnabled = false;
            wnd.ShowDialog();

            _loadedItem = this.ReturnName;
            if(ReturnTechpool == null || ReturnTechpool.Length == 0)
            {
                _currentItemTable = null;
                return;
            }
            _currentItemTable = new DeserializedItemTable(ReturnTechpool);
            CreateTechpoolMenu(_currentItemTable, sender, e);
            _selectedItemLabel.Content = (object)_loadedItem;
        }
        private void LoadModelTechpoolTemplate(object sender, RoutedEventArgs e)
        {
            //_loadedItem = "modeltechpool.bin";
            //_currentItemTable = new DeserializedItemTable(File.ReadAllBytes("modeltechpool.bin"));

            ModelSelectionWindow wnd = new ModelSelectionWindow(this);
            wnd.ShowDialog();

            _loadedItem = this.ReturnName;
            if (ReturnTechpool == null || ReturnTechpool.Length == 0)
            {
                _currentItemTable = null;
                return;
            }
            _currentItemTable = new DeserializedItemTable(ReturnTechpool);

            CreateTechpoolMenu(_currentItemTable, sender, e);
            _selectedItemLabel.Content = (object)_loadedItem;
        }
        private void LoadTrimTechpoolTemplate(object sender, RoutedEventArgs e)
        {
            //_loadedItem = "trimtechpool.bin";
            //_currentItemTable = new DeserializedItemTable(File.ReadAllBytes("trimtechpool.bin"));

            TrimSelectionWindow wnd = new TrimSelectionWindow(this);
            wnd.ShowDialog();

            _loadedItem = this.ReturnName;
            if (ReturnTechpool == null || ReturnTechpool.Length == 0)
            {
                _currentItemTable = null;
                return;
            }
            _currentItemTable = new DeserializedItemTable(ReturnTechpool);

            CreateTechpoolMenu(_currentItemTable, sender, e);
            _selectedItemLabel.Content = (object)_loadedItem;
        }
        private void CreateTechpoolMenu(DeserializedItemTable itemTable, object sender, RoutedEventArgs e)
        {
            ClearParameters();
            var ct = itemTable.ContentTable;
            object ci;
            var h = _techPoolGrid.Height;
            _techPoolGrid.Height *= 10;
            Int32 i = 0;
            foreach (var key in ct.Keys)
            {
                ci = ct[key];
                if (ci.GetType() != typeof(double)) return;

                //Console.WriteLine("Key: " + (string)key + " Param: " + (double)ci);

                TechpoolParameter tp = new TechpoolParameter { Name = (string)key, Parameter = (double)ci };
                TechpoolParameterTemplate tpt = new TechpoolParameterTemplate(tp, _parameterEntryTemplate);

                tpt.Grid.SetValue(Grid.RowProperty, i);
                var m = tpt.Grid.Margin;
                m.Top = i * ParamTemplateHeight;
                tpt.Grid.Margin = m;

                _techpoolGrid.Children.Add(tpt.Grid);
                _loadedTechpoolGrids.Add(tpt.Grid);
                _originalParameters.Add((string)key, (double)ci);
                _newParameters.Add((string)key, (double)ci);
                _applyChangesButton.IsEnabled = false;
                _resetChangesButton.IsEnabled = false;
                _modelTechpoolButton.IsEnabled = true;
                _trimTechpoolButton.IsEnabled = true;
                _familyTechpoolButton.IsEnabled = true;
                _variantTechpoolButton.IsEnabled = true;
                i++;
            }
            _techPoolGrid.Height = h;
        }
        private void ApplyChanges(object sender, RoutedEventArgs e)
        {
            if (_currentItemTable == null)
                return;
            

            Hashtable ct = _currentItemTable.ContentTable;
            foreach(var key in _newParameters.Keys)
            {
                ct[key] = _newParameters[key];
                _originalParameters[key] = _newParameters[key];
            }
            byte[] serialized = _currentItemTable.Serialize();


            //File.WriteAllBytes(_loadedItem, serialized); // Replace with writing to database <-------
            switch (ItemType)
            {
                case LoadedItemType.Family:
                    DBManager.SetFamilyTechpool(ReturnUID, serialized);
                    break;
                case LoadedItemType.Variant:
                    DBManager.SetVariantTechpool(ReturnUID, serialized);
                    break;
                case LoadedItemType.Model:
                    DBManager.SetModelTechpool(ReturnUID, serialized);
                    break;
                case LoadedItemType.Trim:
                    DBManager.SetTrimTechpool(ReturnUID, serialized);
                    break;
                default:
                    throw new ArgumentException("Unknown item type", "ItemType");
            }

            _applyChangesButton.IsEnabled = false;
            _resetChangesButton.IsEnabled = false;
            _modelTechpoolButton.IsEnabled = true;
            _trimTechpoolButton.IsEnabled = true;
            _familyTechpoolButton.IsEnabled = true;
            _variantTechpoolButton.IsEnabled = true;
        }
        private void ResetLoadedParameters(object sender, RoutedEventArgs e)
        {
            //_currentItemTable = new DeserializedItemTable(this.ReturnTechpool);
            switch (ItemType) 
            {
                case LoadedItemType.Family:
                    _currentItemTable = new DeserializedItemTable(DBManager.GetFamilyTechpool(ReturnUID));
                    break;
                case LoadedItemType.Variant:
                    _currentItemTable = new DeserializedItemTable(DBManager.GetVariantTechpool(ReturnUID));
                    break;
                case LoadedItemType.Model:
                    _currentItemTable = new DeserializedItemTable(DBManager.GetModelTechpool(ReturnUID));
                    break;
                case LoadedItemType.Trim:
                    _currentItemTable = new DeserializedItemTable(DBManager.GetTrimTechpool(ReturnUID));
                    break;
            }
            CreateTechpoolMenu(_currentItemTable, sender, e);
        }
        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(this).ShowDialog();
        }
        private void ClearParameters()
        {
            foreach(var item in _loadedTechpoolGrids)
            {
                _techpoolGrid.Children.Remove(item);
                //_currentItemTable = null;
            }
            _loadedTechpoolGrids.Clear();
            //_currentItemTable = null;
            _originalParameters.Clear();
            _newParameters.Clear();
        }
        private bool CheckUpdatedValues()
        {
            bool isUnapplied = false;
            foreach(var key in _newParameters.Keys)
            {
                if (Math.Round(_newParameters[key]) != Math.Round(_originalParameters[key]))
                {
                    isUnapplied = true;
                    //Console.WriteLine($"New Parameter: {{Key: {key} Value: {Math.Round(_newParameters[key])}}}");
                    //Console.WriteLine($"Old Parameter: {{Key: {key} Value: {Math.Round(_originalParameters[key])}}}");
                }

                if (isUnapplied)
                {
                    _applyChangesButton.IsEnabled = true;
                    _resetChangesButton.IsEnabled = true;
                    _modelTechpoolButton.IsEnabled = false;
                    _trimTechpoolButton.IsEnabled = false;
                    _familyTechpoolButton.IsEnabled = false;
                    _variantTechpoolButton.IsEnabled = false;
                }
                else
                {
                    _applyChangesButton.IsEnabled = false;
                    _resetChangesButton.IsEnabled = false;
                    _modelTechpoolButton.IsEnabled = true;
                    _trimTechpoolButton.IsEnabled = true;
                    _familyTechpoolButton.IsEnabled = true;
                    _variantTechpoolButton.IsEnabled = true;
                }
            }
            return isUnapplied;
        }
        private void CheckForUpdates()
        {
            try
            {
                WebClient wc = new WebClient();

                string sdata = wc.DownloadString("https://pastebin.com/raw/ZX2z1ZvT");
                UInt32 idata = UInt32.Parse(sdata);

                if (idata > VERSION)
                {
                    string link = wc.DownloadString("https://pastebin.com/raw/cjAYQdEJ");
                    MessageBox.Show($"A newer version is available! Check out {link} to download it",
                        "Outdated version",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                wc.Dispose();
            }
            catch
            {

            }
        }
    }
}
