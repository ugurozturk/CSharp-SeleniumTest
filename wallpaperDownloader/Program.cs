using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Threading;

namespace wallpaperDownloader
{
    internal class Program
    {
        static string dbName = "..\\..\\myDB.sqlite";
        //155
        private static void Main(string[] args)
        {
            dbOlustur();
            ChromeOptions options = new ChromeOptions();
            options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            options.AddArguments("headless");
            IWebDriver driver = new ChromeDriver(options);

            int pageNumber = sonGirilenSayfa();
            while (true)
            {
                Console.WriteLine("Getting in : https://alpha.wallhaven.cc/search?q=&categories=111&purity=100&atleast=1920x1080&ratios=16x9&sorting=date_added&order=asc&colors=999999&page=" + pageNumber);
                driver.Navigate().GoToUrl("https://alpha.wallhaven.cc/search?q=&categories=111&purity=100&atleast=1920x1080&ratios=16x9&sorting=date_added&order=asc&colors=999999&page=" + pageNumber);
                //for (int i = 0; i < pageNumber; i++)
                //{
                //    ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight - 150)");
                //    Thread.Sleep(10000);
                //}
                pageNumber++;
                sqlYaz("PageNumber", pageNumber.ToString());

                Console.WriteLine("Getting Urls");
                IList<IWebElement> all = driver.FindElements(By.ClassName("preview"));
                List<string> all_urls = new List<string>();
                foreach (var item in all)
                {
                    all_urls.Add(item.GetAttribute("href"));
                }
                try
                {
                    foreach (var url in all_urls)
                    {
                        Console.WriteLine("Gettin in : " + url);
                        string name = url.Substring(url.LastIndexOf('/') + 1);
                        string rootPath = @"D:\wallhaven16-9";
                        if (!File.Exists(rootPath + "\\" + name + ".png"))
                        {
                            driver.Navigate().GoToUrl(url);
                            IWebElement my_image = driver.FindElement(By.Id("wallpaper"));
                            string img_link = my_image.GetAttribute("src");
                            System.Drawing.Image image = DownloadImageFromUrl(img_link);
                            string fileName = System.IO.Path.Combine(rootPath, name + ".png");
                            image.Save(fileName);
                            Console.WriteLine("Saved : " + rootPath + "\\" + name + ".png");
                            sqlYaz("SavedImage", name);
                        }
                        else
                        {
                            Console.WriteLine("Skipped (Already Exist) : " + url);
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Error... Sleeping for 30 sec.");
                    Thread.Sleep(30000);
                }
            }
        }

        private static Image DownloadImageFromUrl(string imageUrl)
        {
            Image image = null;
            try
            {
                System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(imageUrl);
                webRequest.AllowWriteStreamBuffering = true;
                webRequest.Timeout = 30000;

                System.Net.WebResponse webResponse = webRequest.GetResponse();

                Stream stream = webResponse.GetResponseStream();

                image = Image.FromStream(stream);

                webResponse.Close();
            }
            catch (Exception ex)
            {
                return null;
            }

            return image;
        }

        static bool dbOlustur()
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + dbName + ";Version=3;"))
            {
                conn.Open();
                string sql = "CREATE TABLE IF NOT EXISTS degerler (key_name varchar(20), key_value TEXT)";
                SQLiteCommand command = new SQLiteCommand(sql, conn);
                command.ExecuteNonQuery();
                conn.Close();
            }

            return true;
        }

        static bool sqlYaz(string keyname, string keyvalue)
        {

            //Yoksa oluşturacaktır.
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + dbName + ";Version=3;"))
            {
                conn.Open();
                string sql = $"INSERT INTO degerler(key_name,key_value) VALUES ('{keyname}','{keyvalue}')";
                SQLiteCommand command = new SQLiteCommand(sql, conn);
                command.ExecuteNonQuery();
                conn.Close();
            }

            return true;

        }

        static int sonGirilenSayfa()
        {
            int LastPage = 1;


            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + dbName + ";Version=3;"))
            {
                conn.Open();
                string sql = $"select MAX(CAST(key_value AS INT)) as pagenumber from degerler group by key_name having key_name like 'PageNumber'";
                SQLiteCommand command = conn.CreateCommand();

                command.CommandText = sql;
                SQLiteDataReader r = command.ExecuteReader();
                if (r.HasRows)
                {
                    while (r.Read())
                    {
                        LastPage = Convert.ToInt32(r["pagenumber"]);
                    }
                }
                conn.Close();
            }
            return LastPage;
        }
    }
}