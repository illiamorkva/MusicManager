using System;
using System.Windows;
using System.Windows.Input;

namespace MusicManager
{
    /// <summary>
    /// Логика взаимодействия для Oprog.xaml
    /// </summary>
    public partial class Oprog : Window
    {
        public Oprog()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
