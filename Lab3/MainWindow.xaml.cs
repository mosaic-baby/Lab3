using System.Windows;

namespace Lab3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // ДОБАВИТЬ СТРОГО СЮДА:
            this.DataContext = new GameViewModel();
        }
    }
}