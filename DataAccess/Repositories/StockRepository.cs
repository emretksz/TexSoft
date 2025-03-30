using Core.DataAccess.EntityFramework.Repository;
using DataAccess.EntityFramework;
using DataAccess.Interfaces;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class StockRepository : Repository<Stock, TexSoftContext>, IStockDal
    {
        //public StockRepository(TexSoftContext context):base(context)
        //{

        //}
        public async  Task<Stock> GetStockAndProduct(long colorId, long productId)
        {
            using (TexSoftContext db = new TexSoftContext())
            {
                var result = db.Stocks.Include(a => a.Colors).Include(a => a.Product).Where(a => a.ColorId == colorId && a.ProductId == productId).FirstOrDefault();
                return result;
            }
        }
    }

}
