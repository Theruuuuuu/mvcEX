using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models
{
    public class Esg
    {
        public int Id { get; set; }
        public string CompanyNumber { get; set; }
        public string CompanyName { get; set; }
        public string SusESG { get; set; }
        public string MsciESG { get; set; }
        public string FtseESG { get; set; }
        public string IssESG { get; set; }
        public string SapESG { get; set; }
        public string TwCompanyRank { get; set; }
        public string Refi { get; set; }
    }
}
