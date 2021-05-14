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
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace YouTubeThumbnailDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string m_Path;

        private static readonly Regex m_URLMatch = new Regex(@"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)", RegexOptions.IgnoreCase);
        private static readonly string m_URLError = "The URL is not youtube one. maybe.";
        private static readonly Regex m_HTMLTitleMatch = new Regex(@"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase);

        private static readonly string m_ThumbnailUrlFormat_Def = @"https://img.youtube.com/vi/{0}/default.jpg";
        private static readonly string m_ThumbnailUrlFormat_HQ  = @"https://img.youtube.com/vi/{0}/hqdefault.jpg";
        private static readonly string m_ThumbnailUrlFormat_MQ  = @"https://img.youtube.com/vi/{0}/mqdefault.jpg";
        private static readonly string m_ThumbnailUrlFormat_SD  = @"https://img.youtube.com/vi/{0}/sddefault.jpg";
        private static readonly string m_ThumbnailUrlFormat_MAX = @"https://img.youtube.com/vi/{0}/maxresdefault.jpg";

        private string id;

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            m_Path = @"C:\Users\" + System.Environment.UserName + @"\Downloads";

            UpdateDirectoryTextBox(m_Path);
        }

        private void Button_SelectDirectory_Click(object sender, RoutedEventArgs e)
        {
            using (var fdilog = new FolderBrowserDialog()
            {
                Description = "Please choose directory to save.",
                SelectedPath = m_Path,
            }) {
                if (fdilog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                m_Path = fdilog.SelectedPath;
                UpdateDirectoryTextBox(m_Path);
            }
        }

        private void UpdateDirectoryTextBox(string path)
        {
            TextBox_Directory.Text = path;
        }

        private void Button_GetInfo_Click(object sender, RoutedEventArgs e)
        {
            var url = TextBox_URL.Text;
            var urlRegMatch = m_URLMatch.Match(url);
            if (!urlRegMatch.Success)
            {
                Label_Title.Content = m_URLError;
                return;
            }
            id = urlRegMatch.Groups[1].Value;

            string webSource;
            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                webSource = webClient.DownloadString(url);
            }
            var title = m_HTMLTitleMatch.Match(webSource).Groups["Title"].Value;
            if(title != null)
            {
                Label_Title.Content = title;
            }
            BitmapImage imageSource = new BitmapImage(new Uri(string.Format(m_ThumbnailUrlFormat_MQ, id)));
            Image_Thumbnail.Source = imageSource;
            Button_SaveThumbnail.IsEnabled = true;
        }

        private void Button_SaveThumbnail_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(id))
            {
                Button_SaveThumbnail.IsEnabled = false;
                return;
            }
            if (string.IsNullOrEmpty(m_Path))
            {
                Button_SaveThumbnail.IsEnabled = false;
                return;
            }
            if (!Directory.Exists(m_Path))
            {
                Label_Title.Content = "Incorrect path.";
                Button_SaveThumbnail.IsEnabled = false;
                return;
            }
            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                webClient.DownloadFile(string.Format(m_ThumbnailUrlFormat_MAX, id), m_Path+@"\"+Label_Title.Content+"_"+id+".jpg");
            }
            Button_SaveThumbnail.IsEnabled = false;
        }

        private void TextBox_URL_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(Button_SaveThumbnail == null) { return; }
            Button_SaveThumbnail.IsEnabled = false;
            id = "";
        }
    }
}
