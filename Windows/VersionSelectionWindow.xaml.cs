using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace TechpoolUnleashed.Windows
{
    /// <summary>
    /// Interaction logic for VersionSelectionWindow.xaml
    /// </summary>
    public partial class VersionSelectionWindow : Window
    {
        Grid _mainGrid;
        ComboBox _comboBox;
        Button _selectButton;

        MainWindow _mainWindow;

        public VersionSelectionWindow(MainWindow wnd)
        {
            _mainWindow = wnd;

            InitializeComponent();

            _mainGrid = (Grid)this.FindName("MainGrid");
            _comboBox = (ComboBox)_mainGrid.FindName("ComboBox");
            _selectButton = (Button)_mainGrid.FindName("SelectButton");

            _selectButton.IsEnabled = false;

            //((ComboBoxItem)_comboBox.SelectedItem).Content
            _comboBox.SelectionChanged += _comboBox_SelectionChanged;
            _selectButton.Click += _selectButton_Click;
        }

        private void _selectButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath;
            if(_comboBox.SelectedIndex == 1)
            {
                filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                    + "\\AutomationGame\\Saved\\UserData\\";

                if (File.Exists(filePath + "Sandbox_211122.db"))
                {
                    filePath += "Sandbox_211122.db";
                    _mainWindow.DBFilePath = filePath;
                    this.Close();
                    return;
                }

                var files = Directory.GetFiles(filePath);
                foreach(var file in files)
                {
                    if (file.EndsWith(".db"))
                    {
                        if (file.Split('\\').Last().StartsWith("Sandbox"))
                            filePath = file;
                    }
                }

                if (!File.Exists(filePath))
                {
                    ShowNoFileWarning();
                    _mainWindow.DBFilePath = "";
                    this.Close();
                    return;
                }
                else
                {
                    _mainWindow.DBFilePath = filePath;
                    this.Close();
                    return;
                }
            }
            else if (_comboBox.SelectedIndex == 2)
            {
                filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    + "\\My Games\\Automation\\";

                if (File.Exists(filePath + "Sandbox_openbeta.db"))
                {
                    filePath += "Sandbox_openbeta.db";
                    _mainWindow.DBFilePath = filePath;
                    this.Close();
                    return;
                }
                else
                {
                    ShowNoFileWarning();
                    filePath = "";
                    _mainWindow.DBFilePath = filePath;
                    this.Close();
                    return;
                }
            }
            else
            {
                _mainWindow.DBFilePath = "";
                this.Close();
                return;
            }
        }

        private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_comboBox.SelectedIndex == 0)
                _selectButton.IsEnabled = false;

            else
                _selectButton.IsEnabled = true;
        }

        private void ShowNoFileWarning()
        {
            MessageBox.Show("Could not find savefile. Please select a file manually",
                "Could not find database",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
