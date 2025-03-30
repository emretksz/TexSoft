using Core.Entities.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class AllStockList : BaseEntity, IEntity
    {
        public long ProductId { get; set; }
        public long ColorId { get; set; }
        public long Count { get; set; }
        public string Code { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
