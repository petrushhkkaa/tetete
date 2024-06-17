using RC_Cars.Class;
using RC_Cars.DataBase;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace RC_Cars.Pagess
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationView.xaml
    /// </summary>
    public partial class AuthorizationView : Page
    {
        public AuthorizationView()
        {
            InitializeComponent();
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtLogin.Text) || !string.IsNullOrEmpty(pswrdBox.Password))
            {
                var userAuth = Entities.GetContext().Users.FirstOrDefault(p => p.Login == txtLogin.Text && p.Password == pswrdBox.Password);
                if (userAuth != null)
                {
                    CheckClass.idRole = (int)userAuth.IdRole;
                    StartTransition();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль", "Информация", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Заполните все поля", "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            txtLogin.Focus();
        }

        private void pswrdBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnEnter_Click(sender, e);
            }
        }

        private void btnEnter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnEnter_Click(sender, e);
            }
        }

        private void StartTransition()
        {
            var storyboard = (Storyboard)FindResource("PageTransition");
            storyboard.Completed += Transition_Completed;
            storyboard.Begin();
        }

        private void Transition_Completed(object sender, System.EventArgs e)
        {
            NavigationClass.mainFrame.Navigate(new HomeView());
        }
    }
}
