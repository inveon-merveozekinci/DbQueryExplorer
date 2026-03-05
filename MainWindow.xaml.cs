using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DbQueryExplorer.ViewModels;

namespace DbQueryExplorer;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // For DateTime columns apply yyyy-MM-dd HH:mm:ss format
        if (e.PropertyType == typeof(DateTime) || e.PropertyType == typeof(DateTime?))
        {
            var col = new DataGridTextColumn
            {
                Header  = e.Column.Header,
                Binding = new Binding(e.PropertyName)
                {
                    StringFormat = "yyyy-MM-dd HH:mm:ss"
                }
            };
            e.Column = col;
        }
        // DataView exposes columns as DataRowView — detect by DataTable column type
        else if (e.Column is DataGridTextColumn txtCol &&
                 txtCol.Binding is Binding b)
        {
            if (sender is DataGrid dg &&
                dg.ItemsSource is DataView dv &&
                dv.Table!.Columns.Contains(e.PropertyName.ToString()))
            {
                var dataCol = dv.Table.Columns[e.PropertyName.ToString()!]!;
                if (dataCol.DataType == typeof(DateTime))
                {
                    b.StringFormat = "yyyy-MM-dd HH:mm:ss";
                }
            }
        }
    }

    private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.Password = PasswordInput.Password;
    }
}
