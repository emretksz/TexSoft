﻿using Core.DataAccess.EntityFramework.Repository;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface  IStockDal: IRepository<Stock>
    {

        public Task<Stock> GetStockAndProduct(long colorId, long productId);
        
    }
}
