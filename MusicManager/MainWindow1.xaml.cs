using Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WMPLib;

namespace MusicManager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow1 : Window
    {
        internal WindowsMediaPlayer Wmp;
        private string regex = @"[0-9]", _playerUml;
        private bool _pausePlay;
        private IFilter<Sound> _filter;
        private Music objectMusic;
        System.Timers.Timer timer = new System.Timers.Timer(1000);

        public MainWindow1()
        {
            InitializeComponent();
            objectMusic = new Music();
            Wmp = new WindowsMediaPlayer();
            timer.Elapsed += timer_Elapsed;
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                slider1.Maximum = Convert.ToInt32(Wmp.currentMedia.duration);
                slider1.Value = Convert.ToInt32(Wmp.controls.currentPosition);

                if (Wmp != null)
                {
                    int s = (int)Wmp.currentMedia.duration;
                    int h = s / 3600;
                    int m = (s - (h * 3600)) / 60;
                    s = s - (h * 3600 + m * 60);
                    lab_music_stat1.Content = $"{h:D}:{m:D2}:{s:D2}";

                    s = (int)Wmp.controls.currentPosition;
                    h = s / 3600;
                    m = (s - (h * 3600)) / 60;
                    s = s - (h * 3600 + m * 60);
                    lab_music_stat2.Content = $"{h:D}:{m:D2}:{s:D2}";
                }
                else
                {
                    lab_music_stat2.Content = "0:00:00";
                    lab_music_stat1.Content = "0:00:00";
                }
            }));
        }


        // Search
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            idmusic.Visibility = Visibility.Visible;
            idprogres.Visibility = Visibility.Visible;

            int selectSearch = searchcombo.SelectedIndex;
            switch (selectSearch)
            {
                case 0:
                    FullFilters fullFilters = new FullFilters
                    {
                        SearchPatternIspol = text_ispol.Text,
                        SearchPatternName = textname.Text,
                        SearchPatternYear = textyear.Text,
                        SearchPatternZanre = text_zanr.Text
                    };
                    _filter = fullFilters;
                    break;
                case 1:
                    IspolFilters ispolFilters = new IspolFilters { SearchPattern = text_ispol.Text };
                    _filter = ispolFilters;
                    break;
                case 2:
                    ZanreFilters zanreFilters = new ZanreFilters { SearchPattern = text_zanr.Text };
                    _filter = zanreFilters;
                    break;
                case 3:
                    NameFilters nameFilters = new NameFilters { SearchPattern = textname.Text };
                    _filter = nameFilters;
                    break;
                case 4:
                    YearFilters yearFilters = new YearFilters { SearchPattern = textyear.Text };
                    _filter = yearFilters;
                    break;
                case 5:
                    PartialFilters partialFilters = new PartialFilters
                    {
                        SearchPatternIspol = text_ispol.Text,
                        SearchPatternName = textname.Text,
                        SearchPatternYear = textyear.Text,
                        SearchPatternZanre = text_zanr.Text
                    };
                    _filter = partialFilters;
                    break;

            }

            string format = ".mp3";
            if (_mp4.IsChecked == true)
            {
                format = ".mp4";
            }

            //async call search function
            var fullfilesPath = await SearchMusic(format);
            objectMusic.ClearCollection();

            foreach (var c in fullfilesPath)
            {
                string filePath = c.FullName;
                using (FileStream fs = File.OpenRead(filePath))
                {
                    if (fs.Length >= 128)
                    {
                        MusicID3Tag tag = new MusicID3Tag();

                        fs.Seek(-128, SeekOrigin.End);
                        fs.Read(tag.TAGID, 0, tag.TAGID.Length);
                        fs.Read(tag.Title, 0, tag.Title.Length);
                        fs.Read(tag.Artist, 0, tag.Artist.Length);
                        fs.Read(tag.Album, 0, tag.Album.Length);
                        fs.Read(tag.Year, 0, tag.Year.Length);
                        fs.Read(tag.Duration, 0, tag.Duration.Length);
                        fs.Read(tag.Genre, 0, tag.Genre.Length);
                        string theTagid = Encoding.Default.GetString(tag.TAGID);

                        if (theTagid.Equals("TAG"))
                        {

                            string title = Encoding.Default.GetString(tag.Title);
                            string artist = Encoding.Default.GetString(tag.Artist);
                            string album = Encoding.Default.GetString(tag.Album);
                            string year = Encoding.Default.GetString(tag.Year);
                            string genre = Encoding.Default.GetString(tag.Genre);
                            string Duration = Encoding.Default.GetString(tag.Duration);

                            int n = artist.IndexOf("\0");
                            if (n != -1)
                                artist = artist.Remove(n);
                            artist = artist.Trim();
                            n = genre.IndexOf("\0");
                            if (n != -1)
                                genre = genre.Remove(n);
                            genre = genre.Trim();
                            n = title.IndexOf("\0");
                            if (n != -1)
                                title = title.Remove(n);
                            title = title.Trim();

                            Sound sound = new Sound
                            {
                                Author = artist,
                                Genre = genre,
                                Name = title,
                                Path = filePath,
                                Year = year,
                                Format = "MP 3"
                            };
                            if (format == ".mp4")
                            {
                                sound.Format = "MP 4";
                            }
                            if (_filter.Filter(sound))
                            {
                                objectMusic.AddMusic(sound);
                            }
                        }
                    }
                }
            }
            dataGridView1.ItemsSource = objectMusic.GetSounds;
            dataGridView1.Items.Refresh();
            idmusic.Visibility = Visibility.Hidden;
            idprogres.Visibility = Visibility.Hidden;
        }


        async Task<List<FileInfo>> SearchMusic(string f)
        {
            return await Task.Run(() =>
            {
                FileInfo[] fileInfos;
                List<FileInfo> fullfilesPath = new List<FileInfo>();
                String[] logicalDrives = Environment.GetLogicalDrives();
                foreach (string ld in logicalDrives)
                {
                    try
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(ld);
                        if (directoryInfo.Exists)
                        {
                            // search in root
                            fileInfos = directoryInfo.GetFiles("*" + f + "*", SearchOption.TopDirectoryOnly);
                            foreach (FileInfo fi in fileInfos)
                            {
                                fullfilesPath.Add(fi);
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    try
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(ld);
                        if (directoryInfo.Exists)
                        {
                            DirectoryInfo[] Dir = directoryInfo.GetDirectories();
                            foreach (DirectoryInfo dir in Dir)
                            {
                                try
                                {
                                    fileInfos = dir.GetFiles("*" + f + "*", SearchOption.AllDirectories);
                                    foreach (FileInfo fi in fileInfos)
                                    {
                                        fullfilesPath.Add(fi);
                                    }
                                }
                                catch
                                {
                                    // ignored
                                }
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
                return fullfilesPath;
            });
        }

        // move window
        private void Border_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        // check input
        private void textstud_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox text = (TextBox)sender;
            if ((Regex.IsMatch(e.Text, regex)) == false)
                //Char.IsDigit(e.Text, 0)
                e.Handled = true;
            text.SelectionStart = text.Text.Length;

        }

        // grid, output
        private void dataGridView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                //current object
                var name = dataGridView1.SelectedCells[0];
                var author = dataGridView1.SelectedCells[1];
                var genre = dataGridView1.SelectedCells[2];
                var format = dataGridView1.SelectedCells[3];
                var path = dataGridView1.SelectedCells[4];

                var cellContent1 = name.Column.GetCellContent(name.Item);
                var cellContent2 = author.Column.GetCellContent(author.Item);
                var cellContent3 = genre.Column.GetCellContent(genre.Item);
                var cellContent4 = format.Column.GetCellContent(format.Item);
                var cellContent5 = path.Column.GetCellContent(path.Item);

                // show information
                lab_name.Content = (cellContent1 as TextBlock)?.Text;
                lab_author.Content = (cellContent2 as TextBlock)?.Text;
                lab_genre.Content = "Жанр " + (cellContent3 as TextBlock)?.Text;
                lab_format.Content = "Формат " + (cellContent4 as TextBlock)?.Text;
                _playerUml = (cellContent5 as TextBlock)?.Text;
                Wmp.URL = _playerUml;
                Wmp.controls.pause();
                timer.Enabled = true;
                lab_music_stat.Content = "Playing";
                _pausePlay = false;

                //for example :)
                img.Source = (cellContent2 as TextBlock)?.Text == "Adriano Celentano" ? new BitmapImage(new Uri(@"C:\Users\Илья\Desktop\Searchmusic\Images\1.jpg")) : null;
            }
        }
        private void Button_delete(object sender, RoutedEventArgs e)
        {
            objectMusic.ClearCollection();
            dataGridView1.ItemsSource = objectMusic.GetSounds;
            dataGridView1.Items.Refresh();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

       private void Button_Click(object sender, RoutedEventArgs e)
        {
            _pausePlay = !_pausePlay;
            if (_pausePlay)
            {
                Wmp.controls.pause();
                lab_music_stat.Content = "Paused";
            }
            if (!_pausePlay)
            {
                Wmp.controls.play();
                lab_music_stat.Content = "Playing";
            }
        }

        private void slider1_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Wmp.controls.currentPosition = slider1.Value;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Oprog oprog = new Oprog();
            oprog.Show();
        }
    }
}
