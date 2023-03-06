using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models
{
    public class WeatherVM
    {
        public string location { get; set; }
        public string Description { get; set; }
        public string Rain { get; set; }
        public int oC { get; set; }
    }
}
