using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
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


namespace Eight
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
    public partial class Product
    {
        public Uri ImageUri
        {
            get
            {
                var imageName = System.IO.Path.Combine(Environment.CurrentDirectory, Image ?? "");
                return System.IO.File.Exists(imageName) ? new Uri(imageName) : new Uri("pack://application:,,,/img/picture.png");
                //return new Uri(System.IO.Path.Combine(Environment.CurrentDirectory, Image ?? ""));
            }
        }

        public string MaterialsList
        {
            get
            {
                var Result = "";
                foreach (var pm in ProductMaterial)
                {
                    Result += (Result == "" ? "" : ", ") + pm.Material.Title;
                }
                return Result;
            }
        }


        public decimal TotalPrice
        {
            get
            {
                decimal Result = 0;
                foreach (var pm in ProductMaterial)
                    Result += pm.Material.Cost;
                return Result;
            }
        }
    }


    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private IEnumerable<Product> _ProductList;
        // тут мы храним текущую страницу
        private int _CurrentPage = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        private void Invalidate() {
            if (PropertyChanged != null)
            
                PropertyChanged(this, new PropertyChangedEventArgs("ProductList"));
        }

        // при смене текущей страницы мы должны перерисовать список (вспоминайте пр INotifyPropertyChanged)
        private int CurrentPage
        {
            get
            {
                return _CurrentPage;
            }
            set
            {
                _CurrentPage = value;
                Invalidate();
            }
        }

        public IEnumerable<Product> ProductList
        {
            get
            {
                var Result = _ProductList;

                switch (SortType)
                {
                    case 1:
                        Result = Result.OrderBy(p => p.Title);
                        break;
                    case 2:
                        Result = Result.OrderByDescending(p => p.Title);
                        break;
                    case 3:
                        Result = Result.OrderByDescending(p => p.ArticleNumber);
                        break;
                    case 4:
                        Result = Result.OrderBy(p => p.ArticleNumber);
                        break;
                }


                return Result
            .Skip(CurrentPage * 20)
            .Take(20);
                //return _ProductList; 
            }
            set
            {
                _ProductList = value;

                Paginator.Children.Clear();

                // добавляем переход на предыдущую страницу
                Paginator.Children.Add(new TextBlock { Text = " < " });

                // в цикле добавляем страницы
                for (int i = 1; i < _ProductList.Count() / 20; i++)
                    Paginator.Children.Add(
                        new TextBlock { Text = " " + i.ToString() + " " });

                // добавляем переход на следующую страницу
                Paginator.Children.Add(new TextBlock { Text = " > " });

                // проходимся в цикле по всем сохданным элементам и задаем им обработчик PreviewMouseDown
                foreach (TextBlock tb in Paginator.Children)
                    tb.PreviewMouseDown += PrevPage_PreviewMouseDown;
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            
            ProductList = Core.DB.Product.ToList();
            this.DataContext = this;
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


        private void PrevPage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            switch ((sender as TextBlock).Text)
            {
                case " < ":
                    // переход на предыдущую страницу с проверкой счётчика
                    if (CurrentPage > 0) CurrentPage--;
                    return;
                case " > ":
                    // переход на следующую страницу с проверкой счётчика
                    if (CurrentPage < _ProductList.Count() / 20) CurrentPage++;
                    return;
                default:
                    // в остальных элементах просто номер странцы
                    // учитываем, что надо обрезать пробелы (Trim)
                    // и то, что номера страниц начинаются с 0
                    CurrentPage = Convert.ToInt32(
                        (sender as TextBlock).Text.Trim()) - 1;
                    return;
            }
        }

        /*
        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage--;
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            CurrentPage++;
        }
        */

        public string[] SortList 
        { get; set; } = {
    "Без сортировки",
    "название по убыванию",
    "название по возрастанию",
    "номер цеха по убыванию",
    "номер цеха по возрастанию",
    "цена по убыванию",
    "цена по возрастанию" };


        private int SortType = 0;

        private void SortTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SortType = SortTypeComboBox.SelectedIndex;
            Invalidate();
        }
    }
}
