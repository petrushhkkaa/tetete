using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using RC_Cars.DataBase;
using System.Data.Entity;
using System.Text.RegularExpressions;

namespace RC_Cars.Pagess
{
    /// <summary>
    /// Interaction logic for AddEditProductView.xaml
    /// </summary>
    public partial class AddEditProductView : Page
    {
        Product product = new Product();

        public AddEditProductView(Product selectedProduct)
        {
            InitializeComponent();
            product = selectedProduct;

            if (selectedProduct != null)
            {
                DataContext = selectedProduct;
            }
            else
            {
                DataContext = null;
            }

            cmbxTypeOfCar.ItemsSource = Entities.GetContext().TypeOfCar.ToList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на заполнение всех полей и корректность введенных данных
            if (string.IsNullOrEmpty(tbNameProduct.Text) ||
                string.IsNullOrEmpty(tbPriceProduct.Text) ||
                string.IsNullOrEmpty(tbCountProduct.Text) ||
                /*string.IsNullOrEmpty(tbSalesProduct.Text)*/
                string.IsNullOrEmpty(tbModelCar.Text) ||
                string.IsNullOrEmpty(cmbxTypeOfCar.Text))
            {
                MessageBox.Show("Заполните все поля", "Не все поля заполнены", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (tbNameProduct.Text.Length > 50)
            {
                MessageBox.Show("Название продукта не может быть длиннее 50 символов", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (!Regex.IsMatch(tbPriceProduct.Text, @"^\d+(\.\d+)?$"))
            {
                MessageBox.Show("Цена должна быть числом", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (!Regex.IsMatch(tbCountProduct.Text, @"^\d+$"))
            {
                MessageBox.Show("Остаток должен быть целым числом", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            //else if (!Regex.IsMatch(tbSalesProduct.Text, @"^\d+$"))
            
                //MessageBox.Show("Количество продаж должно быть целым числом", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
            
            else if (tbModelCar.Text.Length > 5)
            {
                MessageBox.Show("Масштаб машинки не может превышать 5 символов", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (!Regex.IsMatch(tbModelCar.Text, @"\d+/\d+"))
            {
                MessageBox.Show("Масштаб машинки указывается в формате x/x или x/xx", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Product addProduct = new Product();
                addProduct.Name = tbNameProduct.Text;
                addProduct.Price = int.Parse(tbPriceProduct.Text);
                addProduct.Count = int.Parse(tbCountProduct.Text);
                //addProduct.Sales = tbSalesProduct.Text;
                addProduct.Model_Car = tbModelCar.Text;
                addProduct.TypeOfCar = cmbxTypeOfCar.Text;
                addProduct.Status = addProduct.Count != 0 ? "активен" : "не активен";

                if (product == null)
                {
                    Entities.GetContext().Product.Add(addProduct);
                }
                else
                {
                    Entities.GetContext().Entry(product).State = EntityState.Modified;
                }

                try
                {
                    Entities.GetContext().SaveChanges();
                    MessageBox.Show("Успешное сохранение", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService.GoBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void tbSalesProduct_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
        }
    }
}
