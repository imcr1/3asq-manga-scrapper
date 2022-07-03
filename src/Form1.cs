using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using CefSharp;
using HtmlAgilityPack;
using CefSharp.WinForms;
using Newtonsoft.Json.Linq;
using Microsoft.VisualBasic;

namespace _3asq_scrapper
{
    public partial class Form1 : Form
    {
        ChromiumWebBrowser chromiumWebBrowser = new ChromiumWebBrowser();
        String htmlSourece;
        JArray allChapters = new JArray();
        List<String> allChapterIamge = new List<String>();
        String cwd = Directory.GetCurrentDirectory();
        String mangaName;
        String lastDownLoad;
        int doneDown = 0;
        bool isRun = false;
        bool isRev = false;
        bool isLoading = true;
        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        public  JArray Reverse(JArray values)
        {
            JArray reversedValues = new JArray(values);
            reversedValues.Reverse();
            return reversedValues;
        }

        public async void GetInfo(String mangaURL)
        {
            await chromiumWebBrowser.LoadUrlAsync(mangaURL);
            Thread.Sleep(2000);
            var src = await chromiumWebBrowser.GetSourceAsync();
            
            htmlSourece = src.ToString();

            findAllURLS(src.ToString());
            foreach (var chapter in allChapters)
            {
                listBox1.Items.Add(chapter["title"]);
            }
            button2.Visible = true;
            isLoading =false;
            
            
        }
        public void findAllURLS(String src)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(src);
            HtmlNode[] nodes = doc.DocumentNode.SelectNodes("/html/body/div[1]/div/div[1]/div/div[2]/div/div/div/div[1]/div/div[1]/div/div/div[2]/div/ul/li").ToArray();
            foreach (HtmlNode node in nodes)
            {

                var regexURL = new Regex(@"<a href=\""(.*?)\"">");
                var url = regexURL.Match(node.InnerHtml).Groups[1].ToString();
                var regexName = new Regex(@">\n(.*?)</a>");
                var name = regexName.Match(node.InnerHtml).Groups[1].ToString();
                //MessageBox.Show(name);
                //MessageBox.Show(url);
                JObject idk = new JObject(
                 new JProperty("title", name),
               new JProperty("link", url)

                );
                allChapters.Add(idk);


            }



        }

        public void GetAllPhoto(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {

                String pageHtml = reader.ReadToEnd();
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(pageHtml);
                HtmlNode[] nodes = doc.DocumentNode.SelectNodes("/html/body/div[1]/div/div[1]/div/div/div/div/div/div/div[1]/div[2]/div/div/div/div/img").ToArray();
                foreach (HtmlNode node in nodes)
                {
                    String TheHtml = node.InnerHtml.Trim();
                    HtmlAttribute att = node.Attributes["src"];

                    try { allChapterIamge.Add(att.Value.ToString()); } catch (Exception) { Console.WriteLine(node.InnerHtml.Trim()); }

                }



            }
        }



        public void DownloadAll()
        {

        
            if (isRev)
            {
                var revl = allChapters.Reverse().ToList();
                String FileName = revl[listBox1.SelectedIndex]["title"].ToString().Split('-').First().Trim();
                //MessageBox.Show(FileName);
                String chapterName = revl[listBox1.SelectedIndex]["title"].ToString().Split('-').Last().Trim();
                String Path = cwd + $"\\{mangaName}\\{FileName}\\";
                isRun = true;
                button1.Visible = isRun;
                try
                {


                    if (Directory.Exists(Path))
                    {
                        isRun = false;
                        button1.Visible = isRun;
                        System.Diagnostics.Process.Start(Path);
                        MessageBox.Show("هذا الشابتر موجود فعلا !!");
                    }
                    else
                    {
                        Directory.CreateDirectory(Path);

                        foreach (var item in allChapterIamge)
                        {
                            if (isRun)
                            {
                                String photoName = item.Split('/').Last();
                                String downloadpath = Path + photoName;
                                //MessageBox.Show(downloadpath);

                                using (WebClient webClient = new WebClient())
                                {
                                    webClient.DownloadFile(item, downloadpath);
                                }
                                doneDown += 1;
                                label1.Text = $"[-] Donwloading {chapterName} ({doneDown}/{allChapterIamge.Count})";
                            }


                        }

                        isRun = false;
                        button1.Visible = isRun;
                        if (doneDown == allChapterIamge.Count)
                        {
                            label1.Text = $"Done {chapterName}";
                            lastDownLoad = Path;
                            button3.Visible = true;

                        }
                        else
                        {
                            label1.Text = "Stopped Downloading ...";
                        }
                        doneDown = 0;
                        allChapterIamge.Clear();
                    }
                }
                catch (Exception a)
                {
                    MessageBox.Show($"Error {a.Message}");
                }
            }
            else
            {
                String FileName = allChapters[listBox1.SelectedIndex]["title"].ToString().Split('-').First().Trim();
                //MessageBox.Show(FileName);
                String chapterName = allChapters[listBox1.SelectedIndex]["title"].ToString().Split('-').Last().Trim();
                String Path = cwd + $"\\{mangaName}\\{FileName}\\";
               
                isRun = true;
                button1.Visible = isRun;
                try
                {


                    if (Directory.Exists(Path))
                    {
                        isRun = false;
                        button1.Visible = isRun;
                        System.Diagnostics.Process.Start(Path);
                        MessageBox.Show("هذا الشابتر موجود فعلا !!");
                    }
                    else
                    {
                        Directory.CreateDirectory(Path);

                        label1.Text = $"[-] Donwloading {chapterName} ({doneDown}/{allChapterIamge.Count})";
                        foreach (var item in allChapterIamge)
                        {
                            if (isRun)
                            {
                                String photoName = item.Split('/').Last();
                                String downloadpath = Path + photoName;
                                //MessageBox.Show(downloadpath);

                                using (WebClient webClient = new WebClient())
                                {
                                    webClient.DownloadFile(item, downloadpath);
                                }
                                doneDown += 1;
                                label1.Text = $"[-] Donwloading {chapterName} ({doneDown}/{allChapterIamge.Count})";
                            }


                        }

                        isRun = false;
                        button1.Visible = isRun;
                        if (doneDown == allChapterIamge.Count)
                        {
                            label1.Text = $"Done {chapterName}";
                            lastDownLoad = Path;
                            button3.Visible = true;
                        }
                        else
                        {
                            label1.Text = "Stopped Downloading ...";
                        }

                        allChapterIamge.Clear();
                    }
                }
                catch (Exception a)
                {
                    MessageBox.Show($"Error {a.Message}");
                }
            }

          


        }


        String[] loader = new string[] { "|", "/",  "-", "\\" };
        public void LoadingInfoAnimation()
        {
            while (isLoading)
            {
                foreach (var item in loader)
                {
                    label1.Text = $"[{item}] Loading manga Info ...";
                    Thread.Sleep(500);
                }
            }
            label1.Text = "Welcome Weeb ;)";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Thread backgroundThread = new Thread(new ThreadStart(LoadingInfoAnimation));
            backgroundThread.Start();
            button2.Visible = false;
            button3.Visible = false;
            chromiumWebBrowser.Location = new Point(1000, 1000);
            this.Controls.Add(chromiumWebBrowser);
            string input = Interaction.InputBox("What Manga Do You Seek ?", "Riddle ?" , "https://3asq.org/manga/berserk/");
            button1.Visible = isRun;

            
            if (input != "")
            {
                mangaName = input.Split('/')[4];
                label4.Text = mangaName;
                GetInfo(input);
            }
            else
            {
                mangaName = "berserk";
                label4.Text = mangaName;
                GetInfo("https://3asq.org/manga/berserk/");
            }
            
        }



        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (isRun)
            {
                MessageBox.Show("عذرا إنتظر ليكتمل تحميل الفصل الحالي !");
                allChapterIamge.Clear();
            }
            else
            {
                if (isRev)
                {
                    var revl = allChapters.Reverse().ToList();
                    GetAllPhoto(revl[listBox1.SelectedIndex]["link"].ToString());
                }
                else
                {
                    GetAllPhoto(allChapters[listBox1.SelectedIndex]["link"].ToString());
                }
                
                Thread backgroundThread = new Thread(new ThreadStart(DownloadAll));
                backgroundThread.Start();
            }




            //System.Diagnostics.Process.Start(allChapters[listBox1.SelectedIndex]["link"].ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            isRun = false;
            button1.Visible = isRun;
            label1.Text = "Stopped Downloading ...";
        }

        private bool mouseDown;
        private Point lastLocation;

        private void Lable2_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;
        }

        private void Lable2_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }
        private void Lable2_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void label5_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private  void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            if (isRev)
            {
                foreach (var chapter in allChapters)
                {
                    listBox1.Items.Add(chapter["title"]);

                }
                isRev = false;
            }
            else
            {
                foreach (var chapter in allChapters.Reverse())
                {
                    listBox1.Items.Add(chapter["title"]);
                }
                isRev = true;
            }
          

        }

        private void button3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(lastDownLoad);
        }
    }
}
