    using RC_Cars.Class;
    using RC_Cars.DataBase;
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;
    using System.Windows.Threading;
    using Xceed.Document.NET;
    using Xceed.Words.NET;
    using Path = System.IO.Path;

    namespace RC_Cars.Pagess
    {
        public partial class OrderView : Page
        {
            private DispatcherTimer loadingTimer;

            public OrderView()
            {
                InitializeComponent();
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
                loadingTimer = new DispatcherTimer();
                loadingTimer.Interval = TimeSpan.FromSeconds(4);
                loadingTimer.Tick += LoadingTimer_Tick;
                ShowLoadingIndicator(true);
            }

            private void ShowLoadingIndicator(bool show)
            {
                if (show)
                {
                    loadingIndicatorGrid.Visibility = Visibility.Visible;
                    var storyboard = (Storyboard)FindResource("LoadingAnimation");
                    storyboard.Begin();
                    loadingTimer.Start();
                }
                else
                {
                    loadingIndicatorGrid.Visibility = Visibility.Collapsed;
                    var storyboard = (Storyboard)FindResource("LoadingAnimation");
                    storyboard.Stop();
                    loadingTimer.Stop();
                }
            }

            private void LoadingTimer_Tick(object sender, EventArgs e)
            {
                ShowLoadingIndicator(false);
                dgOrder.ItemsSource = Entities.GetContext().Order.ToList();
                DataContext = Entities.GetContext().Order.ToList();
            }

            private void btnBack_Click(object sender, RoutedEventArgs e)
            {
                NavigationClass.mainFrame.Navigate(new HomeView());
            }

            private void btnAdd_Click(object sender, RoutedEventArgs e)
            {

                NavigationClass.mainFrame.Navigate(new AddEditOrderPage());
            }

            private void btnChart_Click(object sender, RoutedEventArgs e)
            {
                NavigationClass.mainFrame.Navigate(new DiagrammViewDirector());
            }

            private void btnPrint_Click(object sender, RoutedEventArgs e)
            {
                ShowLoadingIndicator(true);
                string tempFilePath = Path.GetTempFileName();
                string tempFileName = Path.ChangeExtension(tempFilePath, ".docx");
                var doc = DocX.Create(tempFileName);
                doc.InsertParagraph("Отчет по заказам")
                    .FontSize(20)
                    .Bold()
                    .Alignment = Alignment.center;

                foreach (var order in dgOrder.Items)
                {
                    var orderData = order as Order;
                    if (orderData == null) continue;

                    doc.InsertParagraph($"ID заказа: {orderData.ID_Order}")
                        .FontSize(16)
                        .Bold();
                    doc.InsertParagraph($"Дата сделки: {orderData.Data_Order.ToString("dd.MM.yyyy")}")
                        .FontSize(14);
                    doc.InsertParagraph($"Общая цена: {orderData.TotalPrice}")
                        .FontSize(14);

                    var productTable = doc.AddTable(orderData.ProductOrder.Count + 1, 3);
                    productTable.Rows[0].Cells[0].Paragraphs[0].Append("Наименование");
                    productTable.Rows[0].Cells[1].Paragraphs[0].Append("Цена");
                    productTable.Rows[0].Cells[2].Paragraphs[0].Append("Количество");
    
                    int rowIndex = 1;
                    foreach (var productOrder in orderData.ProductOrder)
                    {
                        productTable.Rows[rowIndex].Cells[0].Paragraphs[0].Append(productOrder.Product.Name);
                        productTable.Rows[rowIndex].Cells[1].Paragraphs[0].Append(productOrder.Product.Price.ToString());
                        productTable.Rows[rowIndex].Cells[2].Paragraphs[0].Append(productOrder.Quantity.ToString());
                        rowIndex++;
                    }
                    doc.InsertTable(productTable);
                    doc.InsertParagraph(); // Добавляем пустую строку между заказами
                }

                doc.Save();
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tempFileName) { UseShellExecute = true });
                ShowLoadingIndicator(false);
            }

            private void DeleteButton_Click(object sender, RoutedEventArgs e)
            {
                if (sender is Button deleteButton && deleteButton.DataContext is Order order)
                {
                    var result = MessageBox.Show($"Вы действительно хотите удалить заказ ID {order.ID_Order}?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            var context = Entities.GetContext();
                            context.Order.Remove(order);
                            context.SaveChanges();
                            dgOrder.ItemsSource = context.Order.ToList();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Произошла ошибка при удалении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
    }

