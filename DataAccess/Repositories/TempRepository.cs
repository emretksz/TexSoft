using Core.DataAccess.EntityFramework.Repository;
using Core.Utilities.Results;
using DataAccess.EntityFramework;
using DataAccess.Helper;
using DataAccess.Interfaces;
using Entities.Concrete;
using Entities.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Entities.Enums.Enums;

namespace DataAccess.Repositories
{
   
    public class TempRepository : Repository<Temp, TexSoftContext>, ITempDal
    {
        private string connectionString = SqlConst.SqlGetir();

        public async Task<List<ShippingProduct>> GetParticalShipping(long shippingId)
        {
            using (TexSoftContext context = new TexSoftContext())
            {
                var result = await (from a in context.Temps
                                    join c in context.Products on a.ProductId equals c.Id
                                    join b in context.Colors on a.ColorId equals b.Id
                                    where a.Count > 0 && a.ShippigId == shippingId && !a.IsFinished
                                    select new ShippingProduct
                                    {
                                        ColorId = b.Id,
                                        ColorName = b.ColorName,
                                        Count = a.Count,
                                        ProductName = c.ModelName,
                                        ProductId = c.Id,
                                        ShippingId = shippingId,
                                        TempId = a.Id,
                                        RenkBarcode = a.RenkBarcode,
                                        ModelCode = c.ModelCode



                                    }).ToListAsync();
                return result;
            }
            return null;
        }
        public async Task<List<ShippingProduct>> GetOrderShippingResult(long shippingId)
        {
            using (TexSoftContext context = new TexSoftContext())
            {

                var result = await (from a in context.Temps
                                    join b in context.Colors on a.ColorId equals b.Id
                                    join c in context.Products on a.ProductId equals c.Id
                                    where a.ProductId == c.Id && b.Id == a.ColorId && a.ShippigId == shippingId && a.IsComplated && !a.IsFinished && !a.IsDeleted
                                    select new ShippingProduct
                                    {
                                        ColorId = b.Id,
                                        ColorName = b.ColorName,
                                        Count = a.Count,
                                        ProductName = c.ModelName,
                                        ProductId = c.Id,
                                        ShippingId = shippingId,
                                        Age = ((Age)(int)c.Age).ToString(),
                                        Gender = c.Gender,
                                        ModelCode = c.ModelCode,
                                        TempId = a.Id
                                    }).ToListAsync();
                return result;
            }
            return null;
        }


        public async Task<List<ShippingProduct>> GetTenantShippingOrderPrice(long? tenantId = null, long? shippingId = null)
        {
            await using (TexSoftContext context = new TexSoftContext())
            {
                List<ShippingProduct> result;

                if (tenantId != null)
                {
                    result = await (from t in context.Temps
                                    join s in context.Shippings on t.ShippigId equals s.Id
                                    join tenant in context.Tenants on s.TenantId equals tenant.Id
                                    join c in context.Colors on t.ColorId equals c.Id
                                    join p in context.Products on t.ProductId equals p.Id
                                    where t.ShippigId == shippingId.Value && !t.IsDeleted && t.IsFinished
                                    select new ShippingProduct
                                    {
                                        ColorId = c.Id,
                                        ColorName = c.ColorName,
                                        Count = t.Count,
                                        ProductName = p.ModelName,
                                        ProductId = t.ProductId,
                                        ShippingId = t.ShippigId,
                                        Age = ((Age)(int)p.Age).ToString(),
                                        Gender = p.Gender,
                                        ModelCode = p.ModelCode,
                                        TenantName = tenant.TenantName,
                                        TenantId = tenant.Id,
                                        TempId = t.Id
                                    }).AsNoTracking().ToListAsync();
                }
                else
                {
                    result = await (from a in context.Shippings
                                    join b in context.Tenants on a.TenantId equals b.Id
                                    join c in context.Temps on a.Id equals c.ShippigId
                                    join p in context.Products on c.ProductId equals p.Id
                                    where !c.IsDeleted
                                    select new ShippingProduct
                                    {
                                        ColorId = c.ColorId,
                                        ColorName = b.TenantName,
                                        Count = c.Count,
                                        ProductName = p.ModelName,
                                        ProductId = c.ProductId,
                                        ShippingId = c.ShippigId,
                                        Age = ((Age)(int)p.Age).ToString(),
                                        Gender = p.Gender,
                                        ModelCode = p.ModelCode,
                                        TenantName = b.TenantName,
                                        TenantId = b.Id,
                                        TempId = c.Id
                                    }).AsNoTracking().ToListAsync();
                }

                List<ShippingProduct> sp = new List<ShippingProduct>();
                HashSet<long> tenantTmp = new HashSet<long>();
                HashSet<long> tenantShippingId = new HashSet<long>();

                foreach (var item in result)
                {
                    AddOrUpdateShippingProduct(sp, item, tenantTmp, tenantShippingId);
                }

                return sp;
            }
            return null;
        }


        private void AddOrUpdateShippingProduct(List<ShippingProduct> sp, ShippingProduct item, HashSet<long> tenantTmp, HashSet<long> tenantShippingId)
        {
            var existingProduct = sp.FirstOrDefault(a => a.TenantId == item.TenantId && a.ShippingId == item.ShippingId);

            if (existingProduct != null)
            {
                existingProduct.ShippingLists.Add(new ShippingList
                {
                    Age = (Age)Convert.ToInt32(item.Age),
                    ColorId = item.ColorId,
                    ColorName = item.ColorName,
                    Count = item.Count,
                    Gender = item.Gender,
                    ModelCode = item.ModelCode,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName
                });
            }
            else
            {
                var newProduct = new ShippingProduct
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ColorName = item.ColorName,
                    Gender = item.Gender,
                    Age = item.Age,
                    Count = item.Count,
                    ModelCode = item.ModelCode,
                    ShippingId = item.ShippingId,
                    TenantName = item.TenantName,
                    TenantId = item.TenantId,
                    TempId = item.TempId,
                    ShippingLists = new List<ShippingList>
            {
                new ShippingList
                {
                    Age = (Age)Convert.ToInt32(item.Age),
                    ColorId = item.ColorId,
                    ColorName = item.ColorName,
                    Count = item.Count,
                    Gender = item.Gender,
                    ModelCode = item.ModelCode,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName
                }
            }
                };
                sp.Add(newProduct);
                tenantTmp.Add(item.TenantId);
                tenantShippingId.Add(item.ShippingId);
            }
        }


        public async Task<List<ShippingProduct>> GetTenantShippingOrderPriceMagaza(long? tenantId = null, long? shippingId = null)
        {
            using (TexSoftContext context = new TexSoftContext())
            {

                List<ShippingProduct> result = new List<ShippingProduct>();

                if (tenantId != null)
                {

                    result = await (from t in context.Temps
                                    join s in context.Shippings on t.ShippigId equals s.Id
                                    join tenant in context.Tenants on s.TenantId equals tenant.Id
                                    join c in context.Colors on t.ColorId equals c.Id
                                    join p in context.Products on t.ProductId equals p.Id
                                    where t.ShippigId == shippingId.Value && !t.IsDeleted && t.IsFinished/*&& s.MagazaMi.Value*/

                                    select new ShippingProduct
                                    {
                                        ColorId = c.Id,
                                        ColorName = c.ColorName,
                                        Count = t.Count,
                                        ProductName = p.ModelName,
                                        ProductId = t.ProductId,
                                        ShippingId = t.ShippigId,
                                        Age = ((Gender)(int)p.Age).ToString(),
                                        Gender = p.Gender,
                                        ModelCode = p.ModelCode,
                                        TenantName = tenant.TenantName,
                                        TenantId = tenant.Id,
                                        TempId = t.Id
                                    }).AsNoTracking().ToListAsync();
                }
                else
                {
                    result = await (from a in context.Shippings
                                    join b in context.Tenants on a.TenantId equals b.Id
                                    join c in context.Temps on a.Id equals c.ShippigId
                                    join p in context.Products on c.ProductId equals p.Id
                                    where /*c.IsComplated && c.IsFinished &&*/ !c.IsDeleted/*&&a.MagazaMi.Value*/
                                    select new ShippingProduct
                                    {
                                        ColorId = c.ColorId,
                                        ColorName = b.TenantName,
                                        Count = c.Count,
                                        ProductName = p.ModelName,
                                        ProductId = c.ProductId,
                                        ShippingId = c.ShippigId,
                                        Age = ((Age)(int)p.Age).ToString(),
                                        Gender = p.Gender,
                                        ModelCode = p.ModelCode,
                                        TenantName = b.TenantName,
                                        TenantId = b.Id,
                                        TempId = c.Id
                                    }).AsNoTracking().ToListAsync();
                }


                List<ShippingProduct> sp = new List<ShippingProduct>();

                List<long> tenantTmp = new List<long>();
                List<long> tenantShippingId = new List<long>();
                foreach (var item in result)
                {
                    ShippingProduct pr = new ShippingProduct();
                    if (sp.Count == 0)
                    {
                        pr.ProductId = item.ProductId;
                        pr.ProductName = item.ProductName;
                        pr.ColorName = item.ColorName;
                        pr.Gender = item.Gender;
                        pr.Age = item.Age;
                        pr.Count = item.Count;
                        pr.ModelCode = item.ModelCode;
                        pr.ShippingId = item.ShippingId;
                        pr.TenantName = item.TenantName;
                        pr.TenantId = item.TenantId;
                        pr.TempId = item.TempId;
                        pr.ShippingLists = new List<ShippingList>()
                       {
                           new ShippingList
                           {
                               Age = (((Age)Convert.ToInt32(item.Age))),
                               ColorId = item.ColorId,
                               ColorName=item.ColorName,
                               Count=item.Count,
                               Gender=item.Gender,ModelCode=item.ModelCode,ProductId=item.ProductId,ProductName=item.ProductName
                           }
                       };
                        sp.Add(pr);
                        tenantTmp.Add(item.TenantId);
                        tenantShippingId.Add(item.ShippingId);
                    }
                    else
                    {
                        if (tenantTmp.Contains(item.TenantId) && tenantShippingId.Contains(item.ShippingId))
                        {
                            var result2 = sp.FirstOrDefault(a => a.TenantId == item.TenantId && a.ShippingId == item.ShippingId);
                            result2.ShippingLists.Add(new ShippingList
                            {
                                Age = (((Age)Convert.ToInt32(item.Age))),
                                ColorId = item.ColorId,
                                ColorName = item.ColorName,
                                Count = item.Count,
                                Gender = item.Gender,
                                ModelCode = item.ModelCode,
                                ProductId = item.ProductId,
                                ProductName = item.ProductName
                            });
                        }
                        else
                        {
                            pr.ProductId = item.ProductId;
                            pr.ProductName = item.ProductName;
                            pr.ColorName = item.ColorName;
                            pr.Gender = item.Gender;
                            pr.Age = item.Age;
                            pr.Count = item.Count;
                            pr.ModelCode = item.ModelCode;
                            pr.ShippingId = item.ShippingId;
                            pr.TenantName = item.TenantName;
                            pr.TenantId = item.TenantId;
                            pr.TempId = item.TempId;
                            pr.ShippingLists = new List<ShippingList>()
                       {
                           new ShippingList
                           {
                               Age = (((Age) Convert.ToInt32(item.Age))),
                               ColorId = item.ColorId,
                               ColorName=item.ColorName,
                               Count=item.Count,
                               Gender=item.Gender,ModelCode=item.ModelCode,ProductId=item.ProductId,ProductName=item.ProductName
                           }
                       };
                            sp.Add(pr);
                            tenantTmp.Add(item.TenantId);
                            tenantShippingId.Add(item.ShippingId);
                        }
                    }

                }


                return sp;
            }
            return null;
        }



        public async Task<List<Product>> GetPrepareShipping(long shippingId)
        {
            using (TexSoftContext context = new TexSoftContext())
            {
                var result = await (from sp in context.ShippingDetails
                                    join p in context.Products on sp.ProductId equals p.Id
                                    join pAges in context.ProductAges on p.ProductAges.Id equals pAges.Id
                                    where sp.ShippinbgId == shippingId
                                    select new Product()
                                    {
                                        Id = sp.ProductId,
                                        ModelCode = p.ModelCode,
                                        ModelName = p.ModelName,
                                        ModelImageUrl = p.ModelImageUrl,
                                        Barcode = p.Barcode,
                                        ProductAges = pAges,
                                        ModelCount = sp.Amount.ToString(),
                                        Gender = p.Gender

                                    }).ToListAsync();

                return result;
            }
        }

        public async Task<List<GetShippingOrderList>> GetShippingList(long shippingId)
        {
            List<GetShippingOrderList> list = new List<GetShippingOrderList>();

            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {
                await sqlConn.OpenAsync();
                using (SqlCommand sqlCmd = new SqlCommand("[dbo].[SP_GetShippingOrderList]", sqlConn))
                {
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("@ShippingId", shippingId);

                    using (SqlDataReader sqlDr = await sqlCmd.ExecuteReaderAsync())
                    {
                        while (await sqlDr.ReadAsync())
                        {
                            var tmp = new GetShippingOrderList
                            {
                                ShippingId = sqlDr.GetInt64("ShippigId"),
                                ProductId = sqlDr.GetInt64("ProductId"),
                                ModelName = sqlDr.GetStringOrNull("ModelName"),
                                ModelCode = sqlDr.GetStringOrNull("ModelCode"),
                                Amount = sqlDr.GetInt64OrDefault("Amount"),
                                Age = sqlDr.GetStringOrNull("Age"),
                                Barcode = sqlDr.GetStringOrNull("Barcode"),
                                Price = sqlDr.GetDecimalOrDefault("Price"),
                                UnitePrice = sqlDr.GetDecimalOrDefault("UnitePrice"),
                                Gender = sqlDr.GetEnum<Gender>("Gender"),
                                TenantName = sqlDr.GetStringOrNull("TenantName"),
                                ColorName = sqlDr.GetStringOrNull("ColorName"),
                                RenkBarcode = sqlDr.GetStringOrNull("RenkBarcode")
                            };

                            // Check for existing item and update the amount if exists
                            var existingItem = list.FirstOrDefault(a => a.ProductId == tmp.ProductId && a.ColorName == tmp.ColorName);
                            if (existingItem != null)
                            {
                                existingItem.Amount += tmp.Amount;
                            }
                            else
                            {
                                list.Add(tmp);
                            }
                        }
                    }
                }
            }

            return list.OrderByDescending(a => a.ProductId).ToList();
        }


        public async Task<List<GetShippingOrderList>> GetFis(long shippingId)
        {
            using (SqlConnection sqlConn = new SqlConnection(connectionString))
                try
                {
                    {
                        SqlDataReader sqlDr = null;
                        sqlConn.Open();

                        SqlCommand sqlCmd = new SqlCommand("[dbo].[SP_Fis]", sqlConn);
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.Add("@ShippingId", SqlDbType.BigInt).Value = shippingId;
                        sqlDr = sqlCmd.ExecuteReader();


                        /////////
                        // List<string>  = new List<string>();
                        List<GetShippingOrderList> list = new List<GetShippingOrderList>();
                        while (sqlDr.Read())
                        {
                            var tmp = new GetShippingOrderList();

                            var pricecheck = sqlDr["fiyat"].ToString() != "" ? sqlDr["fiyat"].ToString() : "0";
                            tmp.ModelName = sqlDr["ad"].ToString() != "" ? sqlDr["ad"].ToString() : "";
                            tmp.ModelCode = sqlDr["kod"].ToString() != "" ? sqlDr["kod"].ToString() : "";
                            tmp.Age = sqlDr["yas"].ToString() != "" ? sqlDr["yas"].ToString() : "";
                            tmp.Price = decimal.Parse(pricecheck, CultureInfo.CurrentCulture);
                            tmp.Gender = sqlDr["cinsiyet"].ToString() != "" ? (Gender)(int)(Convert.ToInt32(sqlDr["cinsiyet"])) : 0;
                            tmp.Amount = sqlDr["miktar"].ToString() != "" ? Convert.ToInt64(sqlDr["miktar"]) : 0;
                            tmp.TenantName = sqlDr["TenantName"].ToString() != "" ? sqlDr["TenantName"].ToString() : "";
                            pricecheck = sqlDr["birim"].ToString() != "" ? sqlDr["birim"].ToString() : "0";
                            tmp.UnitePrice = decimal.Parse(pricecheck, CultureInfo.CurrentCulture);

                            list.Add(tmp);


                        }


                        return list;
                    }
                }

                catch (Exception ex)
                {

                    return null;
                }





        }

        public async Task<ComplatedShippingExcel> ComplatedShippingForExcel(long colorId, long productId)
        {
            using (TexSoftContext context = new TexSoftContext())
            {
                var result = from p in context.Products.Include(a => a.ProductAges)
                             join c in context.Colors on colorId equals c.Id
                             where p.Id == productId && c.Id == colorId
                             select new ComplatedShippingExcel()
                             {
                                 Product = p,
                                 Renk = c.ColorName
                             };

                return await result.FirstOrDefaultAsync();
            }
        }


        public async Task<List<Shippings>> GetShippingsAsyncForGrid(DateTime dateTime)
        {

            try
            {
                var startDate = new DateTime(dateTime.Year, 1, 1);
                var endDate = new DateTime(dateTime.Year, 12, 31, 23, 59, 59, 999);
                using (TexSoftContext db = new TexSoftContext())
                {
                    var result = await (from s in db.Shippings
                                        join t in db.Tenants on s.TenantId equals t.Id
                                        where s.RegisterDate >= startDate && s.RegisterDate <= endDate
                                        select new Shippings()
                                        {
                                            Id = s.Id,
                                            SiparisAdi = s.SiparisAdi,
                                            SiparisTutari = s.SiparisTutari,
                                            RegisterDate = s.RegisterDate,
                                            Tenant = t
                                        }).ToListAsync();
                    return result;

                }
            }
            catch (Exception ex)
            {
                List<Shippings> nullSp = new List<Shippings>();

                return nullSp;
            }
        }

        public async Task<List<ShippingProduct>> UpdateFinishShippingListView(long spId)
        {

            using (TexSoftContext db = new TexSoftContext())
            {
                var result = await (from t in db.Temps
                                    join s in db.Shippings on t.ShippigId equals s.Id
                                    join p in db.Products on t.ProductId equals p.Id
                                    join c in db.Colors on t.ColorId equals c.Id
                                    join st in db.Stocks on new { t.ColorId, t.ProductId } equals new { st.ColorId, st.ProductId }
                                    where t.ShippigId == spId && st.ProductId == t.ProductId && st.ColorId == t.ColorId
                                    select new ShippingProduct()
                                    {
                                        TempId = t.Id,
                                        ProductName = p.ModelName,
                                        Barcode = p.Barcode,
                                        RenkBarcode = db.Stocks.FirstOrDefault(a => a.ProductId == p.Id && a.ColorId == c.Id).RenkBarcode,
                                        ColorName = c.ColorName,
                                        ShippingId = s.Id,
                                        ColorId = t.ColorId,
                                        ProductId = t.ProductId,
                                        Count = t.Count,
                                        ModelCode = p.ModelCode,
                                        TotalCount = db.ShippingDetails.FirstOrDefault(a => a.ShippinbgId == s.Id).ShippingCount.Value,
                                        StockCount = st.StockCount
                                    }).ToListAsync();
                return result;
            }


        }

        public async Task<Temp> RemoveShippingAmount(long spId, long colorId, long productId)
        {

            using (TexSoftContext db = new TexSoftContext())
            {
                var result = db.Temps.Where(a => a.ColorId == colorId && a.ProductId == productId && a.ShippigId == spId).FirstOrDefault();
                return result;
            }
        }

        public async Task<List<Temp>> RemoveShippingProduct(long spId, long productId)
        {
            using (TexSoftContext db = new TexSoftContext())
            {
                var result = db.Temps.Where(a => a.ProductId == productId && a.ShippigId == spId).ToList();
                return result;
            }
        }
    }
    public static class SqlDataReaderExtensions
    {
        public static string GetStringOrNull(this SqlDataReader reader, string columnName)
        {
            var value = reader[columnName];
            return value == DBNull.Value ? null : value.ToString();
        }

        public static long GetInt64OrDefault(this SqlDataReader reader, string columnName)
        {
            var value = reader[columnName];
            return value == DBNull.Value ? 0 : Convert.ToInt64(value);
        }

        public static decimal GetDecimalOrDefault(this SqlDataReader reader, string columnName)
        {
            var value = reader[columnName];
            return value == DBNull.Value ? 0m : Convert.ToDecimal(value);
        }

        public static TEnum GetEnum<TEnum>(this SqlDataReader reader, string columnName) where TEnum : Enum
        {
            var value = reader[columnName];
            return value == DBNull.Value ? default : (TEnum)Enum.ToObject(typeof(TEnum), Convert.ToInt32(value));
        }
    }
}
