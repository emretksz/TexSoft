using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    internal class ShippingAddAndRemove
    {
        public long Id { get; set; }
        public string ModelKodu { get; set; }
        public string UrunAdi { get; set; }
        public int SiparisAdeti { get; set; }
        public string EklenenAdet { get; set; }
    }
}
