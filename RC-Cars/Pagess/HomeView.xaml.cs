using System;
using System.Collections.Generic;
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

namespace RC_Cars.Pagess
{
    /// <summary>
    /// Логика взаимодействия для HomeView.xaml
    /// </summary>
    public partial class HomeView : Page
    {
        public HomeView()
        {
            InitializeComponent();
        }
        private void btnProduct_Click(object sender, RoutedEventArgs e)
        {
            NavigationClass.mainFrame.Navigate(new ProductView());
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationClass.mainFrame.Navigate(new AuthorizationView());
        }

        private void btnOrder_Click(object sender, RoutedEventArgs e)
        {
            NavigationClass.mainFrame.Navigate(new OrderView());
        }
    }
}
