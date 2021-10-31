using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebScrapping.Models
{
    public class CsvOject
    {
        public string Fraction { get; set; }
        public string Symbol { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }

        public string ImgDataSrc { get; set; }
        public string ImgAlt { get; set; }
        public string ImgWidth { get; set; }
        public string ImgHeight { get; set; }
    }
}
