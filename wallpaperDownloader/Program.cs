using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;

namespace wallpaperDownloader
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IWebDriver driver = new ChromeDriver();
            Console.WriteLine("Getting in : https://alpha.wallhaven.cc/search?q=&categories=111&purity=100&atleast=1920x1080&sorting=views&order=desc&colors=999999&page=2");

            int scrollTime = 1;
            while (true)
            {
                driver.Navigate().GoToUrl("https://alpha.wallhaven.cc/search?q=&categories=111&purity=100&atleast=1920x1080&sorting=views&order=desc&colors=999999&page=2");
                for (int i = 0; i < scrollTime; i++)
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight - 150)");
                    Thread.Sleep(10000);
                }
                scrollTime++;

                Console.WriteLine("Getting Urls");
                IList<IWebElement> all = driver.FindElements(By.ClassName("preview"));
                List<string> all_urls = new List<string>();
                foreach (var item in all)
                {
                    all_urls.Add(item.GetAttribute("href"));
                }
                try
                {
                    all_urls.Reverse();
                    foreach (var url in all_urls)
                    {
                        Console.WriteLine("Gettin in : " + url);
                        string name = url.Substring(url.LastIndexOf('/') + 1);
                        string rootPath = @"D:\seleniumTest";
                        if (!File.Exists(rootPath + "\\" + name + ".png"))
                        {
                            driver.Navigate().GoToUrl(url);
                            IWebElement my_image = driver.FindElement(By.Id("wallpaper"));
                            string img_link = my_image.GetAttribute("src");
                            System.Drawing.Image image = DownloadImageFromUrl(img_link);
                            string fileName = System.IO.Path.Combine(rootPath, name + ".png");
                            image.Save(fileName);
                            Console.WriteLine("Saved : " + rootPath + "\\" + name + ".png");
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
    }
}