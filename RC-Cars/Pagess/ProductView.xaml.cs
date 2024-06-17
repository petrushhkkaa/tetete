using RC_Cars.Class;
using RC_Cars.DataBase;
using System;
using System.Data.Entity;
using System.Linq;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Controls;
using Word = Microsoft.Office.Interop.Word;

namespace RC_Cars.Pagess
{
    /// <summary>
    /// Логика взаимодействия для Product.xaml
    /// </summary>
    public partial class ProductView
    {
        public ProductView()
        {
            InitializeComponent();
            dgProduct.ItemsSource = Entities.GetContext().Product.ToList();
            if (CheckClass.idRole == 1)
            {
                spDirector.Visibility = Visibility.Visible;
                spAdmin.Visibility = Visibility.Collapsed;
            }
            else
            {
                spDirector.Visibility = Visibility.Collapsed;
                spAdmin.Visibility = Visibility.Visible;
            }
            var products = Entities.GetContext().Product.ToList();
            foreach (var product in products)
            {
                if (product.Count == 0 && product.Status != "не активен")
                {
                    product.Status = "не активен";
                    Entities.GetContext().Entry(product).State = EntityState.Modified;
                }
            }
            
            dgProduct.ItemsSource = products;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationClass.mainFrame.Navigate(new HomeView());
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddEditProductView addProduct = new AddEditProductView(null);
            NavigationClass.mainFrame.Navigate(new AddEditProductView(null));
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectService = ((Button)sender).DataContext as Product;
            if (selectService.Status != "не активен")
            {
                if (MessageBox.Show("Изменить статус на 'не активный'?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    try
                    {
                        selectService.Status = "не активен";
                        Entities.GetContext().Entry(selectService).State = EntityState.Modified;
                        Entities.GetContext().SaveChanges();
                        MessageBox.Show("Статус изменён");
                        NavigationClass.mainFrame.Navigate(new ProductView());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении. {ex}");
                    }
                }
            }
            else
            {
                MessageBox.Show($"Ошибка при изменении статуса. Невозможно изменить статус товара на 'активный'", "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshData()
        {
            dgProduct.ItemsSource = Entities.GetContext().Product.ToList();
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                RefreshData();
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            ExportWord();
        }
        private void ExportWord()
        {
            Word.Application application = new Word.Application();
            Word.Document document = application.Documents.Add();
            Word.Paragraph titleParagraph = document.Paragraphs.Add();
            Word.Range titleRange = titleParagraph.Range;
            titleRange.Text = "Таблица продуктов";
            titleRange.Bold = 1;
            titleRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
            titleRange.InsertParagraphAfter();
            int rowsCount = dgProduct.Items.Count;
            int columnsCount = dgProduct.Columns.Count;
            Word.Table table = document.Tables.Add(titleRange.Next(), rowsCount + 1, columnsCount);
            table.Borders.Enable = 1;
            for (int col = 0; col < columnsCount; col++)
            {
                if (dgProduct.Columns[col].Header != null)
                {
                    table.Cell(1, col + 1).Range.Text = dgProduct.Columns[col].Header.ToString();
                    table.Cell(1, col + 1).Range.Bold = 1;
                }
                else
                {
                    table.Cell(1, col + 1).Range.Text = "No Header";
                }
            }
            for (int row = 0; row < rowsCount; row++)
            {
                var product = dgProduct.Items[row] as Product;
                if (product != null)
                {
                    table.Cell(row + 2, 1).Range.Text = product.Name;
                    table.Cell(row + 2, 2).Range.Text = product.Price.ToString();
                    table.Cell(row + 2, 3).Range.Text = product.Count.ToString();
                    table.Cell(row + 2, 4).Range.Text = product.Sales.ToString();
                    table.Cell(row + 2, 5).Range.Text = product.Model_Car;
                    table.Cell(row + 2, 6).Range.Text = product.TypeOfCar;
                    table.Cell(row + 2, 7).Range.Text = product.Status.ToString();
                }
            }
            application.Visible = true;
            string fileName = "WordProductData.docx"; // Имя файла
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            document.SaveAs2(path);
        }
    }
}
