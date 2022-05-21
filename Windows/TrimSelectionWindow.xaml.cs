using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TechpoolUnleashed.Collections;
using TechpoolUnleashed.SQLite;


namespace TechpoolUnleashed.Windows
{
    /// <summary>
    /// Interaction logic for VariantSelectionWindow.xaml
    /// </summary>
    public partial class TrimSelectionWindow : Window
    {
        Grid _mainGrid;
        ScrollViewer _scrollView;
        DataGrid _dataGrid;

        Border _nameEntryBorder;
        Grid _nameEntryGrid;
        TextBox _nameEntryBox;

        Border _selectionBorder;
        Button _selectionButton;

        List<TrimData> _trimDataList = new List<TrimData>();
        ObservableCollection<TrimData> _trimDataCollection = new ObservableCollection<TrimData>();
        TableReturnResult3[] _searchResult;

        MainWindow _mainWindow;

        public TrimSelectionWindow(MainWindow wnd)
        {
            _mainWindow = wnd;
            InitializeComponent();

            _mainGrid = (Grid)this.FindName("MainGrid");
            _scrollView = (ScrollViewer)_mainGrid.FindName("Scrollview");
            _dataGrid = (DataGrid)_scrollView.FindName("Datagrid");

            _dataGrid.KeyDown += OnDataGridKeyDown;

            _nameEntryBorder = (Border)_mainGrid.FindName("NameEntryBorder");
            _nameEntryGrid = (Grid)_nameEntryBorder.FindName("NameEntryGrid");
            _nameEntryBox = (TextBox)_nameEntryGrid.FindName("NameEntryBox");


            _nameEntryBox.TextChanged += UpdateSearchResults;

            _selectionBorder = (Border)_mainGrid.FindName("SelectionBorder");
            _selectionButton = (Button)_selectionBorder.FindName("SelectionButton");

            _dataGrid.SelectedCellsChanged += OnSelectedCell;
            _selectionButton.Click += SelectItem;


            this.Closed += OnClosedCalled;

            _dataGrid.DataContext = _trimDataCollection;

            _mainWindow.DBManager.ForceOpenDatabase();

            UpdateSearchResults(null, null);

        }

        private void OnDataGridKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && _selectionButton.IsEnabled)
                SelectItem(null, null);
        }

        private void SelectItem(object sender, RoutedEventArgs e)
        {
            TrimData selectedItem = (TrimData)_dataGrid.SelectedItem;

            //Console.WriteLine(selectedItem.Uid);

            byte[] tp = _mainWindow.DBManager.GetTrimTechpool(selectedItem.Uid);
            //Console.WriteLine(tp);
            _mainWindow.ReturnTechpool = tp;
            _mainWindow.ReturnName = selectedItem.Name;
            _mainWindow.ReturnUID = selectedItem.Uid;
            _mainWindow.ItemType = LoadedItemType.Trim;
            this.Close();
        }

        private void OnSelectedCell(object sender, SelectedCellsChangedEventArgs e)
        {
            //Console.WriteLine(_dataGrid.SelectedItem);
            //Console.WriteLine(_dataGrid.SelectedIndex);
            if (_dataGrid.SelectedIndex > -1)
                _selectionButton.IsEnabled = true;
            else
                _selectionButton.IsEnabled = false;
        }

        private void OnClosedCalled(object sender, EventArgs e)
        {
            _mainWindow.DBManager.ForceCloseDatabase();
        }

        private void UpdateSearchResults(object sender, TextChangedEventArgs e)
        {
            //Console.WriteLine("Updating...");
            //_selectionButton.IsEnabled = false;
            if (_dataGrid.SelectedIndex > -1)
                _selectionButton.IsEnabled = true;
            else
                _selectionButton.IsEnabled = false;

            _trimDataList.Clear();
            string searchText = _nameEntryBox.Text;
            if (searchText == "")
                searchText = "%%";
            else
                searchText += "%";
            _searchResult = _mainWindow.DBManager.FindTrimsLike(searchText);
            foreach (var r in _searchResult)
            {
                var modelName = _mainWindow.DBManager.FindModelName(r.PUID);

                _trimDataList.Add(new TrimData { Name = r.Name, Model = modelName, Uid = r.UID });
            }
            _trimDataCollection = new ObservableCollection<TrimData>(_trimDataList);
            _dataGrid.DataContext = _trimDataCollection;
        }

    }

}
