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
using System.Windows.Forms.DataVisualization.Charting;
using RC_Cars.DataBase;

namespace RC_Cars.Pagess
{
    /// <summary>
    /// Логика взаимодействия для DiagrammViewDirector.xaml
    /// </summary>
    public partial class DiagrammViewDirector : Page
    {
        public DiagrammViewDirector()
        {
            InitializeComponent();
            chartProd.ChartAreas.Add(new ChartArea("main"));

            var currentProd = new Series("Продажи")
            {
                IsValueShownAsLabel = true,
                ChartType = SeriesChartType.Pie
            };
            chartProd.Series.Add(currentProd);

            UpdateChart();
        }

        private void UpdateChart()
        {
            Series currentSeries = chartProd.Series.FirstOrDefault();
            if (currentSeries == null) return;
            currentSeries.Points.Clear();
            var context = Entities.GetContext();
            var productSales = context.ProductOrder
                .GroupBy(po => po.Product.Name)
                .Select(g => new
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(po => po.Quantity)
                })
                .ToList();
            foreach (var productSale in productSales)
            {
                if (productSale.TotalQuantity.HasValue)
                {
                    currentSeries.Points.AddXY(productSale.ProductName, productSale.TotalQuantity.Value);
                }
            }
        }
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog p = new PrintDialog();
            if (p.ShowDialog() == true)
            {
                p.PrintVisual(DiagrammPagePrint, "Печать");
            }

        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationClass.mainFrame.GoBack();
        }
    }
}
