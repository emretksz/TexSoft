using Core.DataAccess.EntityFramework.Repository;
using DataAccess.EntityFramework;
using DataAccess.Interfaces;
using Entities.Concrete;
using Entities.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Entities.Enums.Enums;

namespace DataAccess.Repositories
{
    public class ShippingRepository : Repository<Shippings, TexSoftContext>, IShippingDal
    {
        //public ShippingRepository(TexSoftContext context):base(context)
        //{

        //}

        /******************** Kullanmıyorum fromlar örnek olur kalsın *********************************/
        public async Task<List<ShippingListDto>> GetAllShippigDto() 
        {
            using (TexSoftContext context = new TexSoftContext())
            {

                var result = await (from s in context.Shippings
                                    join c in context.ShippingDetails on s.Id equals c.ShippinbgId
                                    where s.Id==c.ShippinbgId
                                    select new ShippingListDto
                                    {
                                        //TenantId=s.Tenant.Id,
                                        RegisterDate=s.RegisterDate,
                                        ShippingStasus=s.ShippingStasus,
                                        IsComplated=s.IsComplated,
                                         ShippingCount=s.ShippingCount/*,Price=s.Price,*/
                                    }).ToListAsync();



                //var result = await ( from s in context.Shippings 
                //                     join p in context.Products on s.ProductId equals p.Id
                //                     join t in context.Temps on s.Id equals t.ShippigId
                //                     join s2 in context.Stocks on p.Id equals s2.ProductId
                //                     join c in context.Colors on s2.ColorId equals c.Id
                //                     select new ShippingListDto
                //                     {
                //                         IsActive=s.IsActive,IsComplated=s.IsComplated,Price=s.Price,ProductId=s.ProductId,ShippingCount=s2.StockCount.ToString(),ShippingStasus=s.ShippingStasus,
                //                         Product= new  List<Product>
                //                         {
                //                             new Product()
                //                             {
                //                              Id=p.Id,
                //                             Age=p.Age,
                //                             IsActive=p.IsActive,
                //                             ModelCode=p.ModelCode,
                //                             ModelColor=p.ModelColor,
                //                             Gender=p.Gender,
                //                             ModelImageUrl=p.ModelImageUrl,
                //                             ModelName=p.ModelName,
                //                             RegisterDate=p.RegisterDate,
                //                             Stock=new Stock
                //                             {
                //                                 Id=s2.Id,
                //                                 ProductId=s2.ProductId,
                //                                 ColorId=s2.ColorId,
                //                                 RegisterDate=s2.RegisterDate,
                //                                 Colors= new  List<Color>
                //                                 {
                //                                     new Color(){
                //                                      Id=c.Id,
                //                                     ColorName=c.ColorName
                //                                     }
                //                                 },
                //                                 //TenantId=s2.TenantId,
                //                                 StockCount=s2.StockCount,
                //                                 StockStatus=s2.StockStatus,

                //                             }

                //                             }


                //                         },

                //                     }).AsNoTracking().ToListAsync();
                //return result;
            }
            return null;
        }



        /*********************///Sipariş detayının içindeki ürün renk ve sipariş  miktarı*//////////////////
        /***********************  Aynı olan produtları engellemek için *****************************/
        public async Task<List<ShippingDetails>> IndexShipping(long id)
        {
            using (TexSoftContext context = new TexSoftContext())
            {
                var shippingContext =await context.ShippingDetails.Include(a => a.Products).Include(a => a.Shippings).Include(a => a.Color).Where(a=>a.ShippinbgId==id&&a.ShippingCount>0).ToListAsync();
                List<ShippingDetails> detay = new List<ShippingDetails>();
                foreach (var item in shippingContext)
                {
                    if (detay.Count==0)
                    {
                        detay.Add(item);
                    }
                    else
                    {
                        var query = detay.FirstOrDefault(a => a.ProductId == item.ProductId);
                     
                        if (query == null)
                        {
                       
                            detay.Add(item);
                        }
                        else
                        { 
                           var countTemp = query.ShippingCount;
                            detay.Remove(query);
                            item.ShippingCount += countTemp;
                            detay.Add(item);
                        }
                    }
                }
            
                return detay;
            }
        }

        //******* Siparişler sayfasında tüm siparişleri getirmek için*///////////
        public async Task <List<IndexShippingForJson>> IndexIcınGetir(DateTime date)
        {
            using (TexSoftContext context = new TexSoftContext())
            {
                var startDate = new DateTime(date.Year, 1, 1);
                var endDate = new DateTime(date.Year, 12, 31, 23, 59, 59, 999);

                var result = await context.Shippings
                                          .Include(a => a.Tenant)
                                          .Where(a => a.RegisterDate >= startDate && a.RegisterDate <= endDate)
                                          .ToListAsync();



                //var result = await context.Shippings.Include(a => a.Tenant).Where(a=>a.RegisterDate.Date==date.Date).ToListAsync();/*Where(a=>a.IsComplated ==false*//*&&Convert.ToInt64(a.ShippingCount) >0*///).ToListAsync();/*.(a => a.Id == id);*/
                //var result2 = await context.Shippings.Include(a => a.Tenant).Where(a=>a.IsComplated==true).ToListAsync();/*.(a => a.Id == id);*/
                List<IndexShippingForJson> list = new List<IndexShippingForJson>();
                foreach (var item in result)
                {
                    IndexShippingForJson temp = new IndexShippingForJson();
                    temp.Id=item.Id;
                    temp.ShippingCount = item.ShippingCount;
                    temp.RegisterDate= item.RegisterDate;
                    temp.SiparisAdi = item.SiparisAdi;
                    temp.SiparisTutari=item.SiparisTutari;
                    temp.TenantName = item.Tenant.TenantName!=null? item.Tenant.TenantName:"";
                    temp.ShippingStasus = item.ShippingCount=="0"? ShippingStasus.Tamamlandı.ToString():((ShippingStasus)(int)item.ShippingStasus).ToString();
                    
                    list.Add(temp);
                }
                return list.OrderBy(a=>a.RegisterDate).ToList();
            }
        }
        public async Task<List<Shippings>> IndexIcınGetirMagaza()
        {
            using (TexSoftContext context = new TexSoftContext())
            {

                var result = await context.Shippings.Include(a => a.Tenant).Where(a => a.IsComplated == true&& Convert.ToInt64(a.ShippingCount) > 0).ToListAsync();/*.(a => a.Id == id);*/
                //var result2 = await context.Shippings.Include(a => a.Tenant).Where(a=>a.IsComplated==true).ToListAsync();/*.(a => a.Id == id);*/
                return result;
            }
        }


        //********** Gönderdiğim sipariş ıdsindeki product için   onun içine renk ve miktarı Modal fade içinde particial Kullandım bastım  *///////////
        public async Task<List<ShippingProduct>> SiparisİicndekiRenkveMiktarlari(long shippingId,long productId)
        {
            using (TexSoftContext context = new TexSoftContext())
            {
                var renkveMiktar = await (from a in context.ShippingDetails.Include(a => a.Products).Include(a => a.Shippings).Include(a => a.Color).Where(a => a.ShippinbgId == shippingId &&a.ProductId== productId&&a.ShippingCount>0)
                                  select new ShippingProduct()
                                  {
                                      ColorId = a.Color.Id,
                                      ColorName = a.Color.ColorName,
                                      Count = a.ShippingCount.Value,
                                      ProductId = a.ProductId,
                                      ProductName = a.Products.ModelName,
                                      ShippingId = shippingId

                                  }).ToListAsync();
                return renkveMiktar;
            }
        }



    }
}
