using System;
using System.Windows;
using System.Windows.Input;

namespace MusicManager
{
    /// <summary>
    /// Логика взаимодействия для About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        private void BorderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
