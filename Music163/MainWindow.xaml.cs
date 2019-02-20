using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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

namespace Music163
{

    public class Song
    {
        public string id { get; set; }
        public string name { get; set; }
        public string alias { get; set; }

        public string ars { get; set; }

        public string dtt { get; set; }
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Btn_Search_Click(object sender, RoutedEventArgs e)
        {
            var key = System.Web.HttpUtility.UrlEncode(TB_search.Text);
            //search_type 含义
            //1   单曲
            //10  专辑
            //100 歌手
            //1000    歌单
            //1002    用户
            //1004    mv
            //1006    歌词
            //1009    主播电台
            Load(key, 0);
        }

        private void Load(string key, int offset)
        {
            _loading.Visibility = Visibility.Visible;
            Task.Factory.StartNew(() =>
            {
                var realoffset = offset * 20;
                var str = GetJSON("https://api.imjad.cn/cloudmusic/?type=search&s=" + key + "&offset=" + realoffset + "&search_type=1");
                JObject jobj = JObject.Parse(str);
                var result = jobj.GetValue("result");
                var sumcount = Convert.ToInt32(result["songCount"].ToString());
                JArray jarraySongs = result["songs"] as JArray;
                List<Song> songs = new List<Song>();
                foreach (var ja in jarraySongs)
                {
                    Song s = new Song();
                    s.id = ja["id"].ToString();
                    s.name = ja["name"].ToString();
                    var alia = ja["alia"] as JArray;
                    foreach (var aa in alia)
                    {
                        if (string.IsNullOrEmpty(s.alias))
                        {
                            s.alias = aa.ToString();
                        }
                        else
                        {
                            s.alias = s.alias + "," + aa.ToString();
                        }
                    }

                    var ar = ja["ar"] as JArray;
                    foreach (var aar in ar)
                    {
                        if (string.IsNullOrEmpty(s.ars))
                        {
                            s.ars = aar["name"].ToString();
                        }
                        else
                        {
                            s.ars = s.ars + "/" + aar["name"].ToString();
                        }
                    }
                    TimeSpan ts = new TimeSpan(Convert.ToInt64(ja["dt"]) * 10000);
                    s.dtt = ts.Minutes + ":" + ts.Seconds;
                    songs.Add(s);
                }
                App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    DG_FavoList.ItemsSource = songs;
                    DGP_Main.ResetPage(20, offset + 1, sumcount);
                    _loading.Visibility = Visibility.Collapsed;
                }));
            });
        }


        public string GetJSON(string requestUrl)
        {
            string result;
            try
            {
                HttpWebRequest httpWebRequest = WebRequest.Create(requestUrl) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                httpWebRequest.CookieContainer = cookieContainer;
                httpWebRequest.AllowAutoRedirect = true;
                httpWebRequest.Method = "GET";
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.Headers.Add("Authorization", "Basic YWRtaW46YWRtaW4=");
                HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                string text = streamReader.ReadToEnd();
                string empty = string.Empty;
                result = text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "服务连接");
                result = "";
            }
            return result;
        }

        public bool HttpDownload(string url, string path)
        {
            bool flag = File.Exists(path);
            if (flag)
            {
                File.Delete(path);
            }
            string text = System.IO.Path.GetDirectoryName(path) + "\\temp";
            Directory.CreateDirectory(text);
            string text2 = text + "\\" + System.IO.Path.GetFileName(path) + ".temp";
            bool flag2 = File.Exists(text2);
            if (flag2)
            {
                File.Delete(text2);
            }
            bool result;
            try
            {
                FileStream fileStream = new FileStream(text2, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
                HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
                Stream responseStream = httpWebResponse.GetResponseStream();
                byte[] array = new byte[1024];
                for (int i = responseStream.Read(array, 0, array.Length); i > 0; i = responseStream.Read(array, 0, array.Length))
                {
                    fileStream.Write(array, 0, i);
                }
                fileStream.Close();
                responseStream.Close();
                File.Move(text2, path);
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                result = false;
            }
            return result;
        }

        public string GetMP3URL(string jsontext)
        {
            JObject jobject = JObject.Parse(jsontext);
            JArray jarray = JArray.Parse(jobject["data"].ToString());
            JObject jobject2 = JObject.Parse(jarray[0].ToString());
            return jobject2["url"].ToString();
        }

        private void Btn_Setting_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "请选择歌曲下载文件夹";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    System.Windows.MessageBox.Show(this, "文件夹路径不能为空", "提示");
                    return;
                }
                TB_downloadPath.Text = dialog.SelectedPath;
            }

        }

        private void DataGridPaging_GridPaging(object sender, GridPagingEventArgs e)
        {
            var key = System.Web.HttpUtility.UrlEncode(TB_search.Text);
            Load(key, e.PageIndex - 1);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var str = AppDomain.CurrentDomain.BaseDirectory + "download";
            TB_downloadPath.Text = str;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DG_FavoList.SelectedItem == null) return;

            _loading.Visibility = Visibility.Visible;
            var item = DG_FavoList.SelectedItem as Song;
            bool flag = !Directory.Exists(TB_downloadPath.Text);
            if (flag)
            {
                Directory.CreateDirectory(TB_downloadPath.Text);
            }
            var dir = TB_downloadPath.Text + System.IO.Path.DirectorySeparatorChar;
            string text = item.id;

            Task.Factory.StartNew(() =>
            {
                string mp3URL = this.GetMP3URL(this.GetJSON(string.Concat(new object[]
                {
                    "https://api.imjad.cn/cloudmusic/?type=song&id=",
                    text,
                    "&br=320000"
                })));
                string str = item.name + ".mp3";

                string filename = dir + str;

                bool flag2 = this.HttpDownload(mp3URL,filename );
                App.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    _loading.Visibility = Visibility.Collapsed;
                    if (flag2)
                    {
                        MessageBox.Show("下载完成");
                        System.Diagnostics.Process.Start(TB_downloadPath.Text);
                    }
                }));
            });
        }

        private void TB_search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                var key = System.Web.HttpUtility.UrlEncode(TB_search.Text);
                Load(key, 0);
            }
        }
    }
}
