using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebScrapping.Models;

namespace WebScrapping.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            //string categoryUrl = "https://lista.mercadolivre.com.br/veiculos/carros-caminhonetes/gol/#deal_print_id=c1550bb0-ff68-11eb-8a0b-a32aa33f1a14&c_id=special-normal&c_element_order=5&c_campaign=GOL&c_uid=c1550bb0-ff68-11eb-8a0b-a32aa33f1a14";
            ////string categoryUrl = "https://lista.mercadolivre.com.br/moveis-casa/_Deal_casaedecoracao-sala#DEAL_ID=MLB2434&S=landingHubcasa-moveis-e-decoracao&V=7&T=Special-normal&L=SALA_DE_ESTAR&deal_print_id=e72b82b0-0107-11ec-8ff0-ebf22f91748d&c_id=special-normal&c_element_order=6&c_campaign=SALA_DE_ESTAR&c_uid=e72b82b0-0107-11ec-8ff0-ebf22f91748d";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public ActionResult GenerateData(string url)
        {
            var response = CallUrl(url).Result;

            List<CsvOject> linkList = CategoryParseHtml2(response);

            TempData["name"] = url;

            return View(linkList);
        }

        [HttpGet]
        public ActionResult DownloadData(string txtName)
        {
            var response = CallUrl(txtName).Result;

            List<CsvOject> linkList = CategoryParseHtml2(response);

            var products =  GetProductsDetail(linkList);
            this.ExportToCsv(products);

            return this.RedirectToAction("Index");
        }

        private static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            client.DefaultRequestHeaders.Accept.Clear();
            var response = client.GetStringAsync(fullUrl);
            return await response;
        }

        private List<CsvOject> CategoryParseHtml2(string html)
        {
            //ImageGrapper();
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            htmlDoc.OptionEmptyCollection = true;

            var programmerLinks = htmlDoc.DocumentNode.Descendants("li")
            .Where(node => node.GetAttributeValue("class", "").Contains("ui-search-layout__item")).ToList();


            List<CsvOject> wikiLink = new List<CsvOject>();

            int count = 0;
            foreach (var link in programmerLinks)
            {
                try
                {
                    CsvOject csvOject = new CsvOject();

                    try
                    {
                        var priceNodeCount = link.SelectNodes("//div[contains(@class, 'ui-search-price--size-medium ui-search-item__group__element')]").Count;

                        if (priceNodeCount > 0)
                        {
                            var priceNode = link.SelectNodes("//div[contains(@class, 'ui-search-price--size-medium ui-search-item__group__element')]")[count];


                            var fraction = priceNode.SelectNodes("div").Descendants("span").Where(node => node.GetAttributeValue("class", "").Contains("price-tag-fraction")).ToList();
                            csvOject.Fraction = fraction[0].InnerHtml;

                            var symbol = priceNode.SelectNodes("div").Descendants("span").Where(node => node.GetAttributeValue("class", "").Contains("price-tag-symbol")).ToList();
                            csvOject.Symbol = symbol[0].InnerHtml;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    var titleCount = link.SelectNodes("//h2[contains(@class, 'ui-search-item__title ui-search-item__group__element')]").Count;

                    if (titleCount == 0)
                    {
                        var titleCounts = link.SelectNodes("//h2[contains(@class, 'ui-search-item__title')]").Count;
                        if (titleCounts > 0)
                        {
                            csvOject.Title = link.SelectNodes("//h2[contains(@class, 'ui-search-item__title')]")[count].InnerHtml;
                        }
                    }
                    if (titleCount > 0)
                    {
                        csvOject.Title = link.SelectNodes("//h2[contains(@class, 'ui-search-item__title ui-search-item__group__element')]")[count].InnerHtml;
                    }


                    var locationCount = link.SelectNodes("//span[contains(@class, 'ui-search-item__group__element ui-search-item__location')]").Count;

                    if (locationCount > 0)
                    {
                        csvOject.Location = link.SelectNodes("//span[contains(@class, 'ui-search-item__group__element ui-search-item__location')]")[count].InnerHtml;
                    }

                    var imageCount = link.SelectNodes("//img[contains(@class, 'ui-search-result-image__element')]").Count;

                    if (imageCount > 0)
                    {
                        var result__image = link.SelectNodes("//img[contains(@class, 'ui-search-result-image__element')]")[count];

                        csvOject.ImgDataSrc = result__image.Attributes["data-src"].Value;
                        csvOject.ImgAlt = result__image.Attributes["alt"].Value;
                        csvOject.ImgWidth = result__image.Attributes["width"].Value;
                        csvOject.ImgHeight = result__image.Attributes["height"].Value;
                    }

                    count++;
                    wikiLink.Add(csvOject);
                }
                catch (Exception ex)
                {

                }
            }
            return wikiLink;
        }

        private List<string> ParseHtml(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            IEnumerable<HtmlNode> nodes = htmlDoc.DocumentNode.Descendants(0).Where(n => n.HasClass("ui-item__title"));

            IEnumerable<HtmlNode> nodessss = htmlDoc.DocumentNode.SelectNodes("//p[contains(@class, 'ui-item__title')]");

            List<string> wikiLink = new List<string>();


            foreach (var titled in nodessss)
            {
                wikiLink.Add(titled.InnerHtml);
            }
            return wikiLink;
        }

        private void WriteToCsv(List<CsvOject> links)
        {
            StringBuilder sb = new StringBuilder();


            var newLines = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", "Amount", "Symbol", "Location", "Title", "Img Link", "imgAlt", "imgWidth", "imgHeight");
            sb.AppendLine(newLines);
            foreach (var link in links)
            {

                var fraction = link.Fraction;
                var symbol = link.Symbol;
                var location = link.Location;
                var title = link.Title;
                var imgDataSrc = link.ImgDataSrc;
                var imgAlt = link.ImgAlt;
                var imgWidth = link.ImgWidth;
                var imgHeight = link.ImgHeight;

                var newLine = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\"", fraction, symbol, location, title, imgDataSrc, imgAlt, imgWidth, imgHeight);
                sb.AppendLine(newLine);
            }


            var timestamp = DateTime.Now.ToFileTime();

            System.IO.File.WriteAllText("Files/" + timestamp + "links.csv", sb.ToString());
        }

        private void ImageGrapper()
        {

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri("https://willzilt.com/wp-content/uploads/2019/12/medicine-thermometer-tablets-pills-1024x682.jpg"), @"C:\\Users\\HP\\Desktop\\zeezee");
            }
        }

        private void ExportToCsv(DataTable products)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = products.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in products.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field =>
                  string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                sb.AppendLine(string.Join(",", fields));
            }
            byte[] byteArray = ASCIIEncoding.ASCII.GetBytes(sb.ToString());
            Response.Clear();
            Response.Headers.Add("content-disposition", "attachment;filename=ProductDetails.csv");
            Response.ContentType = "application/text";
            Response.Body.WriteAsync(byteArray);
            Response.Body.Flush();
        }

        private DataTable GetProductsDetail(List<CsvOject> links)
        {

            DataTable dtProduct = new DataTable("ProductDetails");
            dtProduct.Columns.AddRange(new DataColumn[4] { new DataColumn("ProductID"),
                                            new DataColumn("ProductName"),
                                            new DataColumn("Price"),
                                            new DataColumn("ProductDescription") });
            foreach (var product in links)
            {
                dtProduct.Rows.Add(product.ImgAlt, product.ImgDataSrc, product.ImgHeight, product.Symbol);
            }

            return dtProduct;
        }
    }
}
