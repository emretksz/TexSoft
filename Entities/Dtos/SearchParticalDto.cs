using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class SearchParticalDto
    {
        public string SearchText { get; set; }
        public int? ay1 { get; set; }
        //public int? ay2{ get; set; }
        //public int? ay3 { get; set; }
        //public int? ay4 { get; set; }
        public int ?gender1 { get; set; }
        //public int ?gender2 { get; set; }
        public bool Search { get; set; }
        public int page { get; set; } = 0;
        public int pageSize { get; set; } = 12;
     
    }
}
