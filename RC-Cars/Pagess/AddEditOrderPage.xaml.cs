using RC_Cars.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using Word = Microsoft.Office.Interop.Word;

namespace RC_Cars.Pagess
{
    public partial class AddEditOrderPage : Page
    {
        private List<ComboBox> comboBoxes = new List<ComboBox>();
        private Order order = null;
        private bool isEditMode = false;

        public AddEditOrderPage()
        {
            InitializeComponent();
            dpDate.DisplayDateEnd = DateTime.Today;
            dpDate.DisplayDateStart = DateTime.Today.AddYears(-1);
        }

        private void AddEquipmentComboBox(Product product)
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;

            ComboBox comboBox = new ComboBox();
            comboBox.Margin = new Thickness(0, 0, 5, 0);
            stackPanel.Children.Add(comboBox);

            Button deleteButton = new Button();
            deleteButton.Content = "Удалить";
            deleteButton.Margin = new Thickness(0, 0, 5, 0);
            deleteButton.Padding = new Thickness(5, 0, 5, 0);
            deleteButton.Width = 100;
            deleteButton.Height = 20;
            deleteButton.Background = Brushes.Red;
            deleteButton.Foreground = Brushes.White;
            deleteButton.Click += (sender, e) =>
            {
                wpEquipment.Children.Remove(stackPanel);
                comboBoxes.Remove(comboBox);
            };

            Button duplicateButton = new Button();
            duplicateButton.Content = "Дублировать товар";
            duplicateButton.Margin = new Thickness(0, 0, 5, 0);
            duplicateButton.Padding = new Thickness(5, 0, 5, 0);
            duplicateButton.Width = 150;
            duplicateButton.Height = 20;
            duplicateButton.Background = Brushes.Green;
            duplicateButton.Foreground = Brushes.White;
            duplicateButton.Click += (sender, e) =>
            {
                AddEquipmentComboBox(comboBox.SelectedItem as Product);
            };

            stackPanel.Children.Add(deleteButton);
            stackPanel.Children.Add(duplicateButton);
            wpEquipment.Children.Add(stackPanel);

            comboBox.ItemsSource = Entities.GetContext().Product.Where(s => s.Status == "активен" && s.Count > 0).ToList();
            comboBox.DisplayMemberPath = "Name";
            comboBox.SelectedItem = product;

            comboBoxes.Add(comboBox);
        }

        private void btnAddEquipment_Click(object sender, RoutedEventArgs e)
        {
            AddEquipmentComboBox(null);
        }

        private void btnDuplicateEquipment_Click(object sender, RoutedEventArgs e)
        {
            if (comboBoxes.Any(cb => cb.SelectedItem != null))
            {
                ComboBox selectedComboBox = comboBoxes.First(cb => cb.SelectedItem != null);
                AddEquipmentComboBox(selectedComboBox.SelectedItem as Product);
            }
            else
            {
                MessageBox.Show("Выберите товар для дублирования.");
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dpDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату сделки.");
                return;
            }

            if (dpDate.SelectedDate > DateTime.Today)
            {
                MessageBox.Show("Дата сделки не может быть в будущем.");
                return;
            }

            if (!comboBoxes.Any(cb => cb.SelectedItem != null))
            {
                MessageBox.Show("Добавьте хотя бы один товар.");
                return;
            }

            try
            {
                order = new Order
                {
                    Data_Order = dpDate.SelectedDate.Value
                };

                Dictionary<string, int> equipmentCounts = new Dictionary<string, int>();
                foreach (ComboBox comboBox in comboBoxes)
                {
                    if (comboBox.SelectedItem != null)
                    {
                        Product selectedEquipment = comboBox.SelectedItem as Product;
                        if (selectedEquipment != null)
                        {
                            var selectedValue = selectedEquipment.Name;

                            if (equipmentCounts.ContainsKey(selectedValue))
                            {
                                equipmentCounts[selectedValue]++;
                            }
                            else
                            {
                                equipmentCounts[selectedValue] = 1;
                            }
                        }
                    }
                }

                foreach (var kvp in equipmentCounts)
                {
                    Product selectedEquipment = comboBoxes.FirstOrDefault(cb => (cb.SelectedItem as Product)?.Name == kvp.Key)?.SelectedItem as Product;

                    if (selectedEquipment != null)
                    {
                        // Проверяем, достаточно ли товара для заказа
                        if (selectedEquipment.Count < kvp.Value)
                        {
                            MessageBox.Show($"Недостаточное количество товара '{selectedEquipment.Name}' для заказа.");
                            return;
                        }

                        // Уменьшаем количество товара в базе данных
                        selectedEquipment.Count -= kvp.Value;

                        order.ProductOrder.Add(new ProductOrder
                        {
                            Product = selectedEquipment,
                            Quantity = kvp.Value
                        });
                    }
                }

                if (!isEditMode)
                {
                    CalculateTotalPrice(order);
                    Entities.GetContext().Order.Add(order);
                }

                Entities.GetContext().SaveChanges();
                MessageBox.Show("Данные успешно сохранены");
                NavigationClass.mainFrame.Navigate(new OrderView());
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения данных: {ex.Message}");
            }
            try
            {
                // Проверка, что заказ успешно сохранен
                if (order != null)
                {
                    // Создание нового документа Word
                    Word.Application wordApp = new Word.Application();
                    Word.Document doc = wordApp.Documents.Add();

                    // Добавление заголовка
                    Word.Paragraph title = doc.Content.Paragraphs.Add();
                    title.Range.Text = "Детали заказа";
                    title.Range.Font.Bold = 1;
                    title.Range.Font.Size = 16;
                    title.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    title.Range.InsertParagraphAfter();

                    // Добавление информации о заказе
                    Word.Paragraph orderInfo = doc.Content.Paragraphs.Add();
                    orderInfo.Range.Text = $"Дата заказа: {order.Data_Order}\n\n";
                    orderInfo.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    orderInfo.Range.InsertParagraphAfter();

                    // Создание таблицы для отображения товаров
                    Word.Table productTable = doc.Tables.Add(orderInfo.Range, order.ProductOrder.Count + 1, 4);
                    productTable.Borders.Enable = 1;

                    // Добавление заголовков столбцов
                    productTable.Cell(1, 1).Range.Text = "Название товара";
                    productTable.Cell(1, 2).Range.Text = "Цена (шт)";
                    productTable.Cell(1, 3).Range.Text = "Количество";
                    productTable.Cell(1, 4).Range.Text = "Общая стоимость";

                    // Заполнение таблицы данными о товарах
                    int row = 2;
                    foreach (var productOrder in order.ProductOrder)
                    {
                        decimal totalPriceForProduct = (decimal)(productOrder.Product.Price * productOrder.Quantity ?? 0);
                        productTable.Cell(row, 1).Range.Text = productOrder.Product.Name;
                        productTable.Cell(row, 2).Range.Text = productOrder.Product.Price.ToString();
                        productTable.Cell(row, 3).Range.Text = productOrder.Quantity.ToString();
                        productTable.Cell(row, 4).Range.Text = totalPriceForProduct.ToString();
                        row++;
                    }

                    // Добавление итоговой цены после таблицы
                    Word.Paragraph totalPriceParagraph = doc.Content.Paragraphs.Add();
                    totalPriceParagraph.Range.Text = $"\nИтоговая цена: {order.TotalPrice}";
                    totalPriceParagraph.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    totalPriceParagraph.Range.InsertParagraphAfter();

                    // Генерация уникального номера для заказа
                    string uniqueOrderNumber = DateTime.Now.ToString("yyyyMMddHHmmss");

                    // Установка имени файла
                    string filename = $"Детали_Заказа_№{uniqueOrderNumber}.docx";

                    // Получение местоположения для сохранения файла от пользователя
                    Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                    saveFileDialog.FileName = filename;
                    saveFileDialog.Filter = "Документ Word (*.docx)|*.docx";
                    saveFileDialog.Title = "Сохранить документ Word";
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        object filePath = saveFileDialog.FileName;
                        doc.SaveAs2(ref filePath);
                        doc.Close();
                        wordApp.Quit();

                        MessageBox.Show("Данные успешно сохранены и документ создан.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения данных: {ex.Message}");
            }
        }
        private string GenerateOrderDetails(Order order)
        {
            // Генерация информации о заказе в виде строки
            string orderInfo = $"Дата заказа: {order.Data_Order}\n" +
                               $"Итоговая цена: {order.TotalPrice}\n\n" +
                               "Список товаров:\n";

            foreach (var productOrder in order.ProductOrder)
            {
                decimal totalPriceForProduct = (decimal)(productOrder.Product.Price * productOrder.Quantity ?? 0);
                orderInfo += $"{productOrder.Product.Name} - Цена: {productOrder.Product.Price} 1шт, Количество: {productOrder.Quantity}\n" +
                     $"Общая стоимость: {totalPriceForProduct}\n\n";
            }

            return orderInfo;
        }
        

        public void CalculateTotalPrice(Order order)
        {
            decimal? totalPrice = 0;

            foreach (var productOrder in order.ProductOrder)
            {
                var product = productOrder.Product;
                if (product != null)
                {
                    totalPrice += product.Price * productOrder.Quantity;
                }
            }

            order.TotalPrice = (decimal)totalPrice;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationClass.mainFrame.Navigate(new OrderView());
        }
    }
}
