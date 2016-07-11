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
    public partial class Main : Window
    {
        internal WindowsMediaPlayer Wmp;
        private string regex = @"[0-9]", _playerUml;
        private bool _pausePlay;
        private IFilter<Sound> _filter;
        private Music objectMusic;
        System.Timers.Timer timer = new System.Timers.Timer(1000);

        public Main()
        {
            InitializeComponent();
            objectMusic = new Music();
            Wmp = new WindowsMediaPlayer();
            timer.Elapsed += TimerElapsed;
        }

        void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                Slider1.Maximum = Convert.ToInt32(Wmp.currentMedia.duration);
                Slider1.Value = Convert.ToInt32(Wmp.controls.currentPosition);

                if (Wmp != null)
                {
                    int s = (int)Wmp.currentMedia.duration;
                    int h = s / 3600;
                    int m = (s - (h * 3600)) / 60;
                    s = s - (h * 3600 + m * 60);
                    LabMusicStat1.Content = $"{h:D}:{m:D2}:{s:D2}";

                    s = (int)Wmp.controls.currentPosition;
                    h = s / 3600;
                    m = (s - (h * 3600)) / 60;
                    s = s - (h * 3600 + m * 60);
                    LabMusicStat2.Content = $"{h:D}:{m:D2}:{s:D2}";
                }
                else
                {
                    LabMusicStat2.Content = "0:00:00";
                    LabMusicStat1.Content = "0:00:00";
                }
            }));
        }


        // Search
        private async void SearchMusicClick(object sender, RoutedEventArgs e)
        {
            Idmusic.Visibility = Visibility.Visible;
            Idprogres.Visibility = Visibility.Visible;

            int selectSearch = Searchcombo.SelectedIndex;
            switch (selectSearch)
            {
                case 0:
                    FullFilters fullFilters = new FullFilters
                    {
                        SearchPatternIspol = TextIspol.Text,
                        SearchPatternName = Textname.Text,
                        SearchPatternYear = Textyear.Text,
                        SearchPatternZanre = TextZanr.Text
                    };
                    _filter = fullFilters;
                    break;
                case 1:
                    IspolFilters ispolFilters = new IspolFilters { SearchPattern = TextIspol.Text };
                    _filter = ispolFilters;
                    break;
                case 2:
                    ZanreFilters zanreFilters = new ZanreFilters { SearchPattern = TextZanr.Text };
                    _filter = zanreFilters;
                    break;
                case 3:
                    NameFilters nameFilters = new NameFilters { SearchPattern = Textname.Text };
                    _filter = nameFilters;
                    break;
                case 4:
                    YearFilters yearFilters = new YearFilters { SearchPattern = Textyear.Text };
                    _filter = yearFilters;
                    break;
                case 5:
                    PartialFilters partialFilters = new PartialFilters
                    {
                        SearchPatternIspol = TextIspol.Text,
                        SearchPatternName = Textname.Text,
                        SearchPatternYear = Textyear.Text,
                        SearchPatternZanre = TextZanr.Text
                    };
                    _filter = partialFilters;
                    break;

            }

            string format = ".mp3";
            if (Mp4.IsChecked == true)
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
                        fs.Read(tag.Tagid, 0, tag.Tagid.Length);
                        fs.Read(tag.Title, 0, tag.Title.Length);
                        fs.Read(tag.Artist, 0, tag.Artist.Length);
                        fs.Read(tag.Album, 0, tag.Album.Length);
                        fs.Read(tag.Year, 0, tag.Year.Length);
                        fs.Read(tag.Duration, 0, tag.Duration.Length);
                        fs.Read(tag.Genre, 0, tag.Genre.Length);
                        string theTagid = Encoding.Default.GetString(tag.Tagid);

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
            DataGridView1.ItemsSource = objectMusic.GetSounds;
            DataGridView1.Items.Refresh();
            Idmusic.Visibility = Visibility.Hidden;
            Idprogres.Visibility = Visibility.Hidden;
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
        private void BorderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        // check input
        private void CheckInputPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox text = (TextBox)sender;
            if ((Regex.IsMatch(e.Text, regex)) == false)
                //Char.IsDigit(e.Text, 0)
                e.Handled = true;
            text.SelectionStart = text.Text.Length;

        }

        // grid, output
        private void DataGridViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                //current object
                var name = DataGridView1.SelectedCells[0];
                var author = DataGridView1.SelectedCells[1];
                var genre = DataGridView1.SelectedCells[2];
                var format = DataGridView1.SelectedCells[3];
                var path = DataGridView1.SelectedCells[4];

                var cellContent1 = name.Column.GetCellContent(name.Item);
                var cellContent2 = author.Column.GetCellContent(author.Item);
                var cellContent3 = genre.Column.GetCellContent(genre.Item);
                var cellContent4 = format.Column.GetCellContent(format.Item);
                var cellContent5 = path.Column.GetCellContent(path.Item);

                // show information
                LabName.Content = (cellContent1 as TextBlock)?.Text;
                LabAuthor.Content = (cellContent2 as TextBlock)?.Text;
                LabGenre.Content = "Жанр " + (cellContent3 as TextBlock)?.Text;
                LabFormat.Content = "Формат " + (cellContent4 as TextBlock)?.Text;
                _playerUml = (cellContent5 as TextBlock)?.Text;
                Wmp.URL = _playerUml;
                Wmp.controls.pause();
                timer.Enabled = true;
                LabMusicStat.Content = "Playing";
                _pausePlay = false;

                //for example :)
                Img.Source = (cellContent2 as TextBlock)?.Text == "Adriano Celentano" ? new BitmapImage(new Uri(@"C:\Users\Илья\Desktop\Searchmusic\Images\1.jpg")) : null;
            }
        }
        private void ButtonClear(object sender, RoutedEventArgs e)
        {
            objectMusic.ClearCollection();
            DataGridView1.ItemsSource = objectMusic.GetSounds;
            DataGridView1.Items.Refresh();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WindowClose(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

       private void PausePlayClick(object sender, RoutedEventArgs e)
        {
            _pausePlay = !_pausePlay;
            if (_pausePlay)
            {
                Wmp.controls.pause();
                LabMusicStat.Content = "Paused";
            }
            if (!_pausePlay)
            {
                Wmp.controls.play();
                LabMusicStat.Content = "Playing";
            }
        }

        private void SliderPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Wmp.controls.currentPosition = Slider1.Value;
        }

        private void MenuItemClick(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.Show();
        }
    }
}
