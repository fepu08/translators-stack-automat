using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExcelDataReader;
using Microsoft.Win32;

namespace SyntaxAnalysisWithSymbolTableWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SyntaxAnalyzerAutomat automat;
        private static char csvSeparator = ';';
        private DataSet result;
        private string path;
        private bool solved = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void original_button_Click(object sender, RoutedEventArgs e)
        {
            if (!input_original_tb.Text.Equals(""))
            {
                string original = input_original_tb.Text;
                automat = new SyntaxAnalyzerAutomat(original);
                solved = false;
                input_converted_tb.Text = automat.Converted;
                ReInitListViewItemsWithSolutions();
                ChangeMessagesLabelContent("Input read and converted successfully", AlertType.SUCCESS);
            }
            else WarningInsertInput();
        }

        private void converted_button_Click(object sender, RoutedEventArgs e)
        {
            if (!input_converted_tb.Text.Equals(""))
            {
                string converted = input_converted_tb.Text;
                input_original_tb.Clear();
                automat = new SyntaxAnalyzerAutomat(converted);
                solved = false;
                ReInitListViewItemsWithSolutions();
                ChangeMessagesLabelContent("Converted text changed successfully", AlertType.SUCCESS);
            }
            else WarningInsertInput();
        }

        private void read_file_button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Excel Workbook|*.csv", ValidateNames = true };
            if (ofd.ShowDialog() == true && ofd.FileName != null)
            {
                path = ofd.FileName;
                symbol_table_data_grid.ItemsSource = CreateDataSource(path);
                DisableDataGridColumnsSorting(symbol_table_data_grid);
                filepath_tb.Text = path;

                if (messages_label.Content.Equals("CSV file read successfully")) ChangeMessagesLabelContent("CSV file changed successfully", AlertType.SUCCESS);
                else ChangeMessagesLabelContent("CSV file read successfully", AlertType.SUCCESS);
                EndSelectFileWarning();
            }

            // EXCEL STUFF
            /*
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Excel Workbook|*.xlsx;*.xls", ValidateNames = true };
            if(ofd.ShowDialog() == true)
            {
                FileStream fs = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read);
                IExcelDataReader reader = ExcelReaderFactory.CreateReader(fs);
                result = reader.AsDataSet();
                sheetCbox.Items.Clear();
                foreach (DataTable dt in result.Tables)
                {
                    sheetCbox.Items.Add(dt.TableName);
                }
                reader.Close();

                changeMessagesLabelContent("Excel workbook opened", AlertType.SUCCESS);
            } else
            {
                changeMessagesLabelContent("Can't open Excel workbook", AlertType.DANGER);
            }
            */

        }

        /* // For Excel
        private void sheetCbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            symbol_table_data_grid.DataContext = result.Tables[sheetCbox.SelectedIndex].DefaultView;
            disableDataGridColumnsSorting(symbol_table_data_grid);
            changeMessagesLabelContent("Sheet changed", AlertType.SUCCESS);
        }
        */

        private void DisableDataGridColumnsSorting(DataGrid dataGridId)
        {
            foreach (DataGridColumn column in dataGridId.Columns)
            {
                column.CanUserSort = false;
            }
        }

        private void ChangeMessagesLabelContent(string message, AlertType alertType = AlertType.DEFAULT)
        {
            if (alertType == AlertType.DEFAULT)
            {
                messages_label.Foreground = Brushes.Black;
            } else if (alertType == AlertType.SUCCESS)
            {
                messages_label.Foreground = Brushes.Green;
            } else if (alertType == AlertType.DANGER)
            {
                messages_label.Foreground = Brushes.Red;
            }
            messages_label.Content = message;
        }

        private ICollection CreateDataSource(string filepath)
        {
            DataTable dt = new DataTable();
            DataRow dr;

            string[] lines = File.ReadAllLines(filepath);
            if(lines.Length > 0)
            {
                // headers
                string firstLine = lines[0];
                string[] headerLabels = firstLine.Split(csvSeparator);
                /*
                foreach(string headerWord in headerLabels)
                {
                    dt.Columns.Add(new DataColumn(headerWord));
                }*/

                for (int i = 0; i < headerLabels.Length; i++)
                {
                    dt.Columns.Add(new DataColumn("Column " + (i + 1)));
                }

                //data
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] dataWords = lines[i].Split(';');
                    dr = dt.NewRow();
                    for (int j = 0; j < dataWords.Length; j++)
                    {
                        dr[j] = dataWords[j];
                    }
                    dt.Rows.Add(dr);
                }
            }

            return new DataView(dt);
        }

        private void WriteSolutionToListView(string solution)
        {
            listView_solution.Items.Add(solution);
        }

        private void WriteAllSolutionsToListView()
        {
            for (int i = 0; i < automat.SolutionSteps.Count; i++)
            {
                listView_solution.Items.Add(automat.SolutionSteps[i]);
            }
        }

        private void ClearListView()
        {
            listView_solution.Items.Clear();
        }

        private void ReInitListViewItemsWithSolutions()
        {
            ClearListView();
            WriteSolutionToListView(automat.GetSolution());
        }

        private void StartSolve(object sender, RoutedEventArgs e)
        {
            if (!solved)
            {
                if (automat == null || automat.Converted.Length < 1) { WarningInsertInput(); }
                else if (path == null) { WarningSelectFile(); }
                else
                {
                    automat.ReadTable(path, csvSeparator);
                    bool resultSuccess = automat.Solve();
                    if (resultSuccess) { ChangeMessagesLabelContent("Correct Input", AlertType.SUCCESS); }
                    else { ChangeMessagesLabelContent("Incorrect Input", AlertType.DANGER); }
                    WriteAllSolutionsToListView();
                    solved = true;
                }
            }
        }

        private void WarningInsertInput()
        {
            input_original_tb.BorderBrush = Brushes.Red;
            input_original_tb.BorderThickness = new Thickness(2);
            input_converted_tb.BorderBrush = Brushes.Red;
            input_converted_tb.BorderThickness = new Thickness(2);
            ChangeMessagesLabelContent("Please insert input", AlertType.DANGER);
        }

        private void EndInsertInputWarning(object sender, KeyEventArgs e)
        {
            input_original_tb.ClearValue(TextBox.BorderBrushProperty);
            input_original_tb.BorderThickness = new Thickness(1);
            input_converted_tb.ClearValue(TextBox.BorderBrushProperty);
            input_converted_tb.BorderThickness = new Thickness(1);
        }

        private void WarningSelectFile()
        {
            ChangeMessagesLabelContent("Please select csv file", AlertType.DANGER);
            open_file_btn.BorderBrush = Brushes.Red;
            open_file_btn.BorderThickness = new Thickness(2);
        }

        private void EndSelectFileWarning()
        {
            open_file_btn.ClearValue(Button.BorderBrushProperty);
            open_file_btn.BorderThickness = new Thickness(1);
        }
    }
}
