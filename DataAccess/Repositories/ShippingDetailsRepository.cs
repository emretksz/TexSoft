using Core.DataAccess.EntityFramework.Repository;
using DataAccess.EntityFramework;
using DataAccess.Interfaces;
using Entities.Concrete;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Entities.Enums.Enums;

namespace DataAccess.Repositories
{
    public class ShippingDetailsRepository : Repository<ShippingDetails, TexSoftContext>, IShippingDetails
    {

        public async Task<List<ShippingConfirmListDto>> GetShippingConfirmList(long shippingId)
        {

            using (TexSoftContext db = new TexSoftContext())
            {
                try
                {
                    var result = from a in db.ShippingDetails
                                        join b in db.Products.Include(a => a.ProductAges) on a.ProductId equals b.Id
                                        where a.ShippinbgId== shippingId
                                        select new ShippingConfirmListDto()
                                        {
                                            ShippingId = a.ShippinbgId,
                                            Age = b.ProductAges != null ? b.ProductAges.Name : "",
                                            ProductName = b.ModelName,
                                            Amount = a.Amount,
                                            Gender = ((Gender)(int)b.Gender).ToString(),
                                            Price = a.Price,
                                            ProductId = b.Id,
                                            ProductModel = b.ModelName,
                                            ProductCode=b.ModelCode,
                                            DetailsId=a.Id,
                                            UnitePrice=a.UnitePrice
                                        };
                    var reee = result.ToList();
                    return reee;
                }
                catch (Exception ex)
                {

                    throw;
                }
            }
      



        }
        public async Task<List<ShippingDetails>> GetAllShippingInclude(long shippingId)
        {

            using (TexSoftContext db = new TexSoftContext())
            {
                try
                {

                    var result =  (from a in db.ShippingDetails
                                 join b in db.Products on a.ProductId equals b.Id
                                 where a.ShippinbgId == shippingId
                                 select new ShippingDetails()
                                 {
                                     Id = a.Id,
                                     ShippinbgId=a.ShippinbgId,
                                     Amount = a.Amount,
                                     Price = a.Price,
                                     Products = b,
                                     UnitePrice=a.UnitePrice

                                 }).AsNoTracking().ToList();
                
                  
                    return  result;

                }
                catch (Exception ex)
                {

                    throw;
                }
            }




        }

        public async Task<List<ShippingDetails>> GetAllColorAndProduct(long spId)
        {
            using (TexSoftContext db = new TexSoftContext())
            {
                try
                {

                    var xd = (from a in db.ShippingDetails
                             join b in db.Products on a.ProductId equals b.Id
                             join s in db.Stocks on b.Id equals s.ProductId
                             where a.ShippinbgId == spId

                             select new ShippingDetails()
                             {
                                 Amount = a.Amount,
                                 ColorId = s.ColorId,
                                 ProductId = b.Id,
                                 Price = a.Price,
                                 ShippinbgId = a.ShippinbgId,
                                 Id = a.Id,
                                 ShippingCount = a.ShippingCount,
                                 SiparislereEklendi = a.SiparislereEklendi,
                                 SiparisTamamlandi = a.SiparisTamamlandi,
                                 UnitePrice=a.UnitePrice

                             }).ToList();

                    //var result = db.ShippingDetails
                    //            .Include(a => a.Color)
                    //            .Include(a => a.Products)
                    //            .Where(a => a.ShippinbgId == spId).ToList();
                    return xd;

                }
                catch (Exception ex)
                {

                    throw;
                }
            }

        }
    }
    
}
