using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models
{
	public class LinepayDTO
	{
        public string id { get; set; }
        public string name { get; set; }
        public string imageUrl { get; set; }
        public int quantity { get; set; }
        public int price { get; set; }
    }
}
