using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace FptCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create an instance of Chrome driver
            IWebDriver browser = new ChromeDriver();
            browser.Navigate().GoToUrl("https://fptshop.com.vn/");

            //Get product brand of fptshop
            List<string> listBrand = new List<string>();
            int iBrand = 1;
            try
            {
                //phone
                while (browser.FindElement(By.XPath("/html/body/header/nav/div/ul/li[1]/div/div/div[1]/ul[1]/li[" + iBrand + "]")).Enabled)
                //laptop
                //while (browser.FindElement(By.XPath("/html/body/header/nav/div/ul/li[2]/div/div/div[1]/ul[1]/li[" + iBrand + "]")).Enabled)
                {
                    iBrand++;
                }
            }
            catch
            {
                iBrand -= 1;
            }
            for (int j = 1; j <= iBrand; j++)
            {
                //phone
                string outerHtml = browser.FindElement(By.XPath("/html/body/header/nav/div/ul/li[1]/div/div/div[1]/ul[1]/li[" + j + "]")).GetAttribute("outerHTML");
                //laptop
                //string outerHtml = browser.FindElement(By.XPath("/html/body/header/nav/div/ul/li[2]/div/div/div[1]/ul[1]/li[" + j + "]")).GetAttribute("outerHTML");
                string prBrand = Regex.Match(outerHtml, "href=\"(.*?)\"").Groups[1].Value;
                if (prBrand == "/dien-thoai/masstel") continue;
                prBrand = "https://fptshop.com.vn" + prBrand;
                listBrand.Add(prBrand);
            }

            // create file csv of phone
            System.IO.StreamWriter writer = new System.IO.StreamWriter("D:/FptCrawler_Phone.csv", true, System.Text.Encoding.UTF8);
            writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7}", "Name", "SKU", "Category", "Brand", "Prices", "Colors", "Images", "Descriptions");

            // create file csv of laptop
            //System.IO.StreamWriter writer = new System.IO.StreamWriter("D:/FptCrawler_Laptop.csv", true, System.Text.Encoding.UTF8);
            //writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7}", "Name", "SKU", "Category", "Brand", "Prices", "Colors", "Images", "Descriptions");

            //get product of a brand
            for (int i = 0; i < listBrand.Count; i++)
            {
                //Go to URL of a brand
                browser.Navigate().GoToUrl(listBrand[i]);

                //Get product links of a brand
                var ProductCount = browser.FindElements(By.CssSelector(".product-sale"));
                List<string> listProductLink = new List<string>();
                foreach (var product in ProductCount)
                {
                    string outerHtml = product.GetAttribute("outerHTML");
                    string productLink = Regex.Match(outerHtml, "href=\"(.*?)\"").Groups[1].Value;
                    productLink = "https://fptshop.com.vn" + productLink;
                    listProductLink.Add(productLink);
                }

                for (int iLink = 0; iLink < listProductLink.Count; iLink++)
                {
                    //Go to URL of a product link
                    browser.Navigate().GoToUrl(listProductLink[iLink]);

                    //Get version of product
                    var prVerCount = browser.FindElements(By.CssSelector(".js--select-item"));
                    List<string> listProductVersionLink = new List<string>();
                    foreach (var iprVer in prVerCount)
                    {
                        string outer = iprVer.GetAttribute("outerHTML");
                        string prVer = Regex.Match(outer, "href=\"(.*?)\"").Groups[1].Value;
                        prVer = "https://fptshop.com.vn" + prVer;
                        listProductVersionLink.Add(prVer);
                    }

                    if (listProductVersionLink.Count == 0)
                    {
                        listProductVersionLink.Add(listProductLink[iLink]);
                    }

                    for (int iVersion = 0; iVersion < listProductVersionLink.Count; iVersion++)
                    {
                        //Go to URL of a product version
                        browser.Navigate().GoToUrl(listProductVersionLink[iVersion]);

                        //Get a product name
                        string prName = browser.FindElement(By.CssSelector(".st-name")).GetAttribute("outerHTML");
                        prName = Regex.Match(prName, "<h1 class=\"st-name\">(.*?)<span class=\"st-sku\">(.*?)</span></h1>").Groups[1].Value;

                        //Get sku
                        string prSKU;
                        try
                        {
                            prSKU = browser.FindElement(By.CssSelector(".st-sku")).GetAttribute("innerText");
                            prSKU = prSKU.Replace("(No.", "").Replace(")", "");
                        }
                        catch
                        {
                            prSKU = "";
                        }

                        //Get Brand
                        string prBrand = browser.FindElement(By.XPath("/html/body/div[2]/main/div/div[1]/div[1]/div/ol/li[3]/a")).GetAttribute("innerText");
                        if (prBrand == "Apple (iPhone)") prBrand = "Apple";

                        //Get category
                        string prCategory = browser.FindElement(By.XPath("/html/body/div[2]/main/div/div[1]/div[1]/div/ol/li[2]/a")).GetAttribute("innerText");
                        prCategory = prCategory + ">" + prBrand;

                        //Get all product images
                        //var imgCount = browser.FindElements(By.CssSelector(".js--slide--full img"));
                        //string prImg = "";
                        //int iImg = 0;
                        //foreach (var pr in imgCount)
                        //{
                        //    string outerHtml = pr.GetAttribute("outerHTML");
                        //    string Img = Regex.Match(outerHtml, "src=\"(.*?)\"").Groups[1].Value;
                        //    if (iImg == imgCount.Count - 1) prImg += Img + "";
                        //    else prImg += Img + ", ";
                        //    iImg++;
                        //}

                        //Get product images
                        string img, prImg;
                        try
                        {
                            img = browser.FindElement(By.CssSelector(".js--slide--full .swiper-slide-active img")).GetAttribute("outerHTML");
                            prImg = Regex.Match(img, "src=\"(.*?)\"").Groups[1].Value;
                        }
                        catch
                        {
                            prImg = "";
                        }

                        //Get product prices
                        string prPrice;
                        try
                        {
                            prPrice = browser.FindElement(By.CssSelector(".st-price-sub")).GetAttribute("innerText");
                        }
                        catch
                        {
                            try
                            {
                                prPrice = browser.FindElement(By.CssSelector(".st-price-main")).GetAttribute("innerText");
                            }
                            catch
                            {
                                prPrice = "0";
                            }
                        }
                        prPrice = prPrice.Replace(".", "");
                        prPrice = prPrice.Replace("₫", "");

                        //Get product colors
                        var prColorCount = browser.FindElements(By.CssSelector(".js--select-color-item p"));
                        string prColor = "";
                        foreach (var iColor in prColorCount)
                        {
                            string Color = iColor.Text;
                            prColor += Color + "; ";
                        }

                        //Get product desciptions
                        string prDescription;
                        try
                        {
                            //Get product descriptions
                            prDescription = "<p>" + browser.FindElements(By.CssSelector(".st-pd-table"))[0].GetAttribute("innerText").Replace("\r\n", "</p><p>") + "</p>";
                            prDescription = prDescription.Replace("\t", ": ");
                            prDescription = prDescription.Replace(", ", " - ");
                        }
                        catch
                        {
                            prDescription = "";
                        }

                        //Write in file csv
                        writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7}", prName, prSKU, prCategory, prBrand, prPrice, prColor, prImg, prDescription);
                    }
                }
            }
            //Close file
            writer.Close();
            Thread.Sleep(3000);
        }
    }
}