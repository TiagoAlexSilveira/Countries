using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Countries.Models
{
    public class LivingCost
    {
        public string city { get; set; }
        public List<Cost> costs { get; set; } //estava aqui IList
        public string currency { get; set; }
    }
}
