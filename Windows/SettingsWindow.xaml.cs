using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace TechpoolUnleashed.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        Grid _grid;
        Button _resetButton;
        TextBox _textBox;

        MainWindow _mainWindow;

        public SettingsWindow(MainWindow wnd)
        {
            _mainWindow = wnd;
            InitializeComponent();

            _grid = (Grid)this.FindName("Grid");
            _resetButton = (Button)_grid.FindName("ResetButton");
            _textBox = (TextBox)_grid.FindName("TextBox");

            _textBox.Text = _mainWindow.DBFilePath;

            _resetButton.Click += _resetButton_Click;
        }

        private void _resetButton_Click(object sender, RoutedEventArgs e)
        {
            switch( MessageBox.Show("This will clear the database path and close the application. Continue?",
                "Reset Database Path",
                MessageBoxButton.YesNo, MessageBoxImage.Warning))
            {
                case MessageBoxResult.Yes:
                    _mainWindow.DBFilePath = "";
                    _mainWindow.SaveSettingsFile();
                    Environment.Exit(0);
                    break;
                default: break;
            }
            
        }
    }
}
