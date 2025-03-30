using Core.DataAccess.EntityFramework.Repository;
using DataAccess.EntityFramework;
using DataAccess.Helper;
using DataAccess.Interfaces;
using Entities.Concrete;
using Entities.Dtos;
using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Entities.Enums.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace DataAccess.Repositories
{
    public class OrderRepository : Repository<Order, TexSoftContext>, IOrderDal
    {
        private string connectionString = SqlConst.SqlGetir();
        public async Task<List<OrderShippingDto>> OrderTenantShippingDto(long tenantId)
        {
            List<OrderShippingDto> SPPP = new List<OrderShippingDto>();
           
            using (TexSoftContext context = new TexSoftContext())
            {
                var result = (from t in context.Temps
                              join s in context.ShippingDetails on t.ShippigId equals s.ShippinbgId
                              join p in context.Products on t.ProductId equals p.Id
                              join a in context.ProductAges on p.ProductAges.Id equals a.Id
                              join c in context.Colors on t.ColorId equals c.Id
                              join tenant in context.Tenants on t.TenantId equals tenant.Id
                              where t.TenantId == tenantId
                              select
                              new OrderShippingDto()
                              {
                                  ModelCode = p.ModelCode,
                                  Age = p.ProductAges != null ? p.ProductAges.Name : "",
                                  Barcode = p.Barcode,
                                  Gender = p.Gender,
                                  //Price = s.Amount,
                                  Amount = t.Count,
                                  TenantName=tenant.TenantName

                              });

                /// her product ıd için renkler ve ürübnler toplanmal ve toplam adet 

                var qqq = await result.ToListAsync();

            }


            return null;
        }


        public async Task<List<OrderShippingDto>> SP_TenantShippingOrderZamanaGore(long tenantId,DateTime start,DateTime end)
        {

            try
            {
                List<OrderShippingDto> SPPP = new List<OrderShippingDto>();
                using (SqlConnection sqlConn = new SqlConnection(connectionString))
                {
                SqlDataReader sqlDr = null;

                    sqlConn.Open();

                    SqlCommand sqlCmd = new SqlCommand("[dbo].[SP_TenantDateFilter]", sqlConn);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    //SqlParameter param = new SqlParameter("@TenantId", tenantId);
                    sqlCmd.Parameters.Add("@TenantId", SqlDbType.BigInt).Value = tenantId;
                    sqlCmd.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = start;
                    sqlCmd.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = end;

                    sqlDr = sqlCmd.ExecuteReader();

                    while (sqlDr.Read())
                    {
                        var tmp = new OrderShippingDto
                        {
                            TenantName = sqlDr["TenantName"].ToString(),
                            ProductName = sqlDr["ModelName"].ToString(),
                            ModelCode = sqlDr["ModelCode"].ToString() ?? null,
                            Age = sqlDr["yas"].ToString() ?? null,
                            Price = decimal.TryParse(sqlDr["tutar"].ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out var price) ? price : 0,
                            Amount = long.TryParse(sqlDr["siparisMiktari"].ToString(), out var amount) ? amount : 0,
                            Barcode = sqlDr["Barcode"].ToString() ?? null,
                            Renk = sqlDr["ColorName"].ToString() ?? null,
                            RenkBarcode = sqlDr["RenkBarcode"].ToString() ?? "",
                            Gender = Enum.TryParse(sqlDr["Gender"].ToString(), out Gender gender) ? gender : 0,
                            ColorId = long.TryParse(sqlDr["ColorId"].ToString(), out var colorId) ? colorId : 0,
                            ProductId = long.TryParse(sqlDr["ProductId"].ToString(), out var productId) ? productId : 0,
                            SpId = long.TryParse(sqlDr["ShippinbgId"].ToString(), out var spId) ? spId : 0
                        };

                        if (SPPP.Count == 0)
                        {
                            SPPP.Add(tmp);
                        }
                        else
                        {
                            var check = SPPP.FirstOrDefault(a => a.ProductId == tmp.ProductId && a.ColorId == tmp.ColorId && a.SpId == tmp.SpId);
                            var checkSp = SPPP.FirstOrDefault(a => a.SpId == tmp.SpId && a.ProductId == tmp.ProductId);

                            if (check != null)
                            {
                                check.Amount += tmp.Amount;
                            }
                            else if (checkSp != null || check == null)
                            {
                                SPPP.Add(tmp);
                            }
                        }
                    }



                    return SPPP;
                }

          
            }
            catch (Exception ex)
            {

                return null;
            }
        }


        public async Task<List<YearMonth>> SP_GenelKazanc()
        {

            List<YearMonth> SPPP = new List<YearMonth>();
            string date = DateTime.Now.Date.Year.ToString();
           using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {
                SqlDataReader sqlDr = null;
                sqlConn.Open();

                SqlCommand sqlCmd = new SqlCommand("[dbo].[SP_GetDayIndexMoney]", sqlConn);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                //SqlParameter param = new SqlParameter("@TenantId", tenantId);
                sqlCmd.Parameters.Add("@Date", SqlDbType.VarChar).Value = date;
                //sqlCmd.Parameters.Add(param);
                sqlDr = sqlCmd.ExecuteReader();

                List<string> productId = new List<string>();
                while (sqlDr.Read())
                {

                    var tmp = new YearMonth();


                    while (sqlDr.Read())
                    {
                        string ay = sqlDr["Ay"].ToString();
                        decimal miktar = decimal.TryParse(sqlDr["Miktar"].ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out decimal parsedMiktar) ? parsedMiktar : 0;

                        var mevcutAy = SPPP.FirstOrDefault(a => a.Ay == ay);

                        if (mevcutAy != null)
                        {
                            mevcutAy.Miktar += miktar;
                        }
                        else
                        {
                            SPPP.Add(new YearMonth { Ay = ay, Miktar = miktar });
                        }
                    }
                }
                return SPPP;
            }
           
        }


       public async Task<List<OrderShippingDto>> GetOrderTenantForExcel(long tenanId, DateTime date, bool fabrikaMi = false,string gender=null)
        {
            List<OrderShippingDto> SPPP = new List<OrderShippingDto>();
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(connectionString))
                {
                    SqlDataReader sqlDr = null;
                    sqlConn.Open();
                    SqlCommand sqlCmd = new SqlCommand();
                    if (!fabrikaMi)
                    {
                        //sqlCmd = new SqlCommand("[dbo].[SP_GetAllTenantDetailsForTenantId]", sqlConn);
                        //sqlCmd.Parameters.Add("@TenantId", SqlDbType.BigInt).Value = tenanId;
                        //sqlCmd.Parameters.Add("@Date", SqlDbType.Date).Value = date;
                        if (gender == "1")
                        {
                            // sqlCmd = new SqlCommand("[dbo].[SP_GetFabrikaAllDokuman]", sqlConn);
                            sqlCmd = new SqlCommand("[dbo].[SP_GetTenantGenderKız]", sqlConn);
                            sqlCmd.Parameters.Add("@TenantId", SqlDbType.BigInt).Value = tenanId;
                            sqlCmd.Parameters.Add("@Date", SqlDbType.Date).Value = date;

                        }
                        else if (gender == "2")
                        {
                            sqlCmd = new SqlCommand("[dbo].[SP_GetTenantGenderErkek]", sqlConn);
                            sqlCmd.Parameters.Add("@TenantId", SqlDbType.BigInt).Value = tenanId;
                            sqlCmd.Parameters.Add("@Date", SqlDbType.Date).Value = date;
                        }
                    }
                    else
                    {
                        sqlCmd = new SqlCommand("[dbo].[SP_GetFabrikaAllDokuman]", sqlConn);
                        sqlCmd.Parameters.Add("@Date", SqlDbType.Date).Value = date;


                    }

                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    //SqlParameter param = new SqlParameter("@TenantId", tenanId);
                    //

                    //qlParameter param = new SqlParameter("@TenantId", tenantId);

                    // sqlCmd.Parameters.Add(param);
                    sqlDr = sqlCmd.ExecuteReader();


                    List<string> productId = new List<string>();
                    while (sqlDr.Read())
                    {
                        var tmp = new OrderShippingDto()
                        {
                            TenantName = sqlDr["TenantName"].ToString(),
                            ProductName = sqlDr["ModelName"].ToString(),
                            ModelCode = GetStringValue(sqlDr["ModelCode"]),
                            Age = GetStringValue(sqlDr["yas"]),
                            Price = GetDecimalValue(sqlDr["tutar"]),
                            Amount = GetLongValue(sqlDr["siparisMiktari"]),
                            Barcode = GetStringValue(sqlDr["Barcode"]),
                            Renk = GetStringValue(sqlDr["ColorName"]),
                            RenkBarcode = GetStringValue(sqlDr["RenkBarcode"]),
                            Gender = GetGenderValue(sqlDr["Gender"]),
                            ColorId = GetLongValue(sqlDr["ColorId"]),
                            ProductId = GetLongValue(sqlDr["ProductId"]),
                            SpId = GetLongValue(sqlDr["ShippingId"])
                        };

                        var existingItem = SPPP.FirstOrDefault(a => a.ProductId == tmp.ProductId && a.ColorId == tmp.ColorId && a.SpId == tmp.SpId);
                        var existingSp = SPPP.FirstOrDefault(a => a.SpId == tmp.SpId && a.ProductId == tmp.ProductId);

                        if (existingItem != null)
                        {
                            existingItem.Amount += tmp.Amount;
                        }
                        else if (existingSp != null)
                        {
                            SPPP.Add(tmp);
                        }
                        else
                        {
                            SPPP.Add(tmp);
                        }
                    }

                    return MergeProductList(SPPP);
                   
                }
            }
            catch (Exception ex )
            {

                return null;
            }

        }
        // Yardımcı Metotlar
        static string GetStringValue(object value) => value?.ToString() ?? null;
        static long GetLongValue(object value) => long.TryParse(value?.ToString(), out long result) ? result : 0;
        static decimal GetDecimalValue(object value) => decimal.TryParse(value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result) ? result : 0;
        static Gender GetGenderValue(object value) => int.TryParse(value?.ToString(), out int result) ? (Gender)result : 0;

        static List<OrderShippingDto> MergeProductList(List<OrderShippingDto> sourceList)
        {
            var mergedList = new List<OrderShippingDto>();
            using (TexSoftContext db = new TexSoftContext())
            {
                foreach (var item in db.Products)
                {
                    var productList = sourceList.Where(a => a.ProductId == item.Id);
                    foreach (var item2 in productList)
                    {
                        var existingItem = mergedList.FirstOrDefault(a => a.ProductId == item2.ProductId && a.ColorId == item2.ColorId);
                        if (existingItem != null)
                        {
                            existingItem.Amount += item2.Amount;
                            existingItem.Price += item2.Price;
                        }
                        else
                        {
                            mergedList.Add(item2);
                        }
                    }
                }
            }
            return mergedList;
        }
        //public async Task<List<OrderShippingDto>> GetOrderTenantForExcel(long tenanId)
        //{



        //}


        public async Task<List<OrderShippingDto>> GetTenantKizErkek(DateTime date,string tenantName, string gender =null)
        {
            List<OrderShippingDto> SPPP = new List<OrderShippingDto>();
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(connectionString))
                {
                    SqlDataReader sqlDr = null;
                    sqlConn.Open();
                    SqlCommand sqlCmd = new SqlCommand();
                        //sqlCmd = new SqlCommand("[dbo].[SP_GetAllTenantDetailsForTenantId]", sqlConn);
                        //sqlCmd.Parameters.Add("@TenantId", SqlDbType.BigInt).Value = tenanId;
                        //sqlCmd.Parameters.Add("@Date", SqlDbType.Date).Value = date;
                        if (gender == "1")
                        {
                            // sqlCmd = new SqlCommand("[dbo].[SP_GetFabrikaAllDokuman]", sqlConn);
                            sqlCmd = new SqlCommand("[dbo].[SP_TenantKiz]", sqlConn);
                            sqlCmd.Parameters.Add("@Date", SqlDbType.Date).Value = date;
                            sqlCmd.Parameters.Add("@TenantName", SqlDbType.VarChar).Value = tenantName;
                       
                        }
                        else if (gender == "2")
                        {
                            sqlCmd = new SqlCommand("[dbo].[SP_TenantErkek]", sqlConn);
                        sqlCmd.Parameters.Add("@Date", SqlDbType.Date).Value = date;
                        sqlCmd.Parameters.Add("@TenantName", SqlDbType.VarChar).Value = tenantName;
                         }
                 

                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    //SqlParameter param = new SqlParameter("@TenantId", tenanId);
                    //

                    //qlParameter param = new SqlParameter("@TenantId", tenantId);

                    // sqlCmd.Parameters.Add(param);
                    sqlDr = sqlCmd.ExecuteReader();


                    List<string> productId = new List<string>();
                    while (sqlDr.Read())
                    {
                        var tmp = new OrderShippingDto();
                        if (SPPP.Count == 0)
                        {
                            tmp.TenantName = sqlDr["TenantName"].ToString();
                            tmp.ProductName = sqlDr["ModelName"].ToString();
                            tmp.ModelCode = sqlDr["ModelCode"].ToString() != "" ? sqlDr["ModelCode"].ToString() : null;
                            tmp.Age = sqlDr["yas"].ToString() != "" ? sqlDr["yas"].ToString() : null;
                            //tmp.Price = sqlDr["tutar"].ToString() != "" ? Convert.ToDecimal(sqlDr["tutar"]) : 0;
                            tmp.Price = sqlDr["tutar"].ToString() != "" ? decimal.Parse(sqlDr["tutar"].ToString(), CultureInfo.CurrentCulture) : 0;
                            tmp.Amount = sqlDr["siparisMiktari"].ToString() != "" ? Convert.ToInt64(sqlDr["siparisMiktari"]) : 0;
                            tmp.Barcode = sqlDr["Barcode"].ToString() != "" ? sqlDr["Barcode"].ToString() : null;
                            tmp.Renk = sqlDr["ColorName"].ToString() != "" ? sqlDr["ColorName"].ToString() : null;
                            tmp.RenkBarcode = sqlDr["RenkBarcode"].ToString() != "" ? sqlDr["RenkBarcode"].ToString() : "";
                            tmp.Gender = sqlDr["Gender"].ToString() != "" ? (Gender)(int)sqlDr["Gender"] : 0;
                            tmp.ColorId = sqlDr["ColorId"].ToString() != "" ? Convert.ToInt64(sqlDr["ColorId"]) : 0;
                            tmp.ProductId = sqlDr["ProductId"].ToString() != "" ? Convert.ToInt64(sqlDr["ProductId"]) : 0;
                            tmp.SpId = sqlDr["ShippinbgId"].ToString() != "" ? Convert.ToInt64(sqlDr["ShippinbgId"]) : 0;
                            SPPP.Add(tmp);
                        }
                        else
                        {
                            var colorsId = sqlDr["ColorId"].ToString() != "" ? Convert.ToInt64(sqlDr["ColorId"]) : 0;
                            var check = SPPP.FirstOrDefault(a => a.ProductId == Convert.ToInt64(sqlDr["ProductId"]) && a.ColorId == colorsId && a.SpId == Convert.ToInt64(sqlDr["ShippinbgId"]));
                            var checkSp = SPPP.FirstOrDefault(a => a.SpId == Convert.ToInt64(sqlDr["ShippinbgId"]) && a.ProductId == Convert.ToInt64(sqlDr["ProductId"]));
                            if (check != null)
                            {
                                var amountCheck = sqlDr["siparisMiktari"].ToString() != "" ? Convert.ToInt64(sqlDr["siparisMiktari"]) : 0;
                                check.Amount = check.Amount + amountCheck;
                                //var tutar = sqlDr["tutar"].ToString() != "" ? Convert.ToDecimal(sqlDr["tutar"]) : 0;
                                //    check.Price = check.Price + tutar;
                            }
                            else if (checkSp != null)
                            {
                                tmp.TenantName = sqlDr["TenantName"].ToString();
                                tmp.ProductName = sqlDr["ModelName"].ToString();
                                tmp.ModelCode = sqlDr["ModelCode"].ToString() != "" ? sqlDr["ModelCode"].ToString() : null;
                                tmp.Age = sqlDr["yas"].ToString() != "" ? sqlDr["yas"].ToString() : null;
                                // tmp.Price = sqlDr["tutar"].ToString() != "" ? Convert.ToDecimal(sqlDr["tutar"]) : 0; yopkk
                                tmp.Amount = sqlDr["siparisMiktari"].ToString() != "" ? Convert.ToInt64(sqlDr["siparisMiktari"]) : 0;
                                tmp.Barcode = sqlDr["Barcode"].ToString() != "" ? sqlDr["Barcode"].ToString() : null;
                                tmp.Renk = sqlDr["ColorName"].ToString() != "" ? sqlDr["ColorName"].ToString() : null;
                                tmp.Gender = sqlDr["Gender"].ToString() != "" ? (Gender)(int)sqlDr["Gender"] : 0;
                                tmp.ColorId = sqlDr["ColorId"].ToString() != "" ? Convert.ToInt64(sqlDr["ColorId"]) : 0;
                                tmp.ProductId = sqlDr["ProductId"].ToString() != "" ? Convert.ToInt64(sqlDr["ProductId"]) : 0;
                                tmp.SpId = sqlDr["ShippinbgId"].ToString() != "" ? Convert.ToInt64(sqlDr["ShippinbgId"]) : 0;
                                tmp.RenkBarcode = sqlDr["RenkBarcode"].ToString() != "" ? sqlDr["RenkBarcode"].ToString() : "";
                                SPPP.Add(tmp);
                            }
                            else
                            {
                                tmp.TenantName = sqlDr["TenantName"].ToString();
                                tmp.ProductName = sqlDr["ModelName"].ToString();
                                tmp.ModelCode = sqlDr["ModelCode"].ToString() != "" ? sqlDr["ModelCode"].ToString() : null;
                                tmp.Age = sqlDr["yas"].ToString() != "" ? sqlDr["yas"].ToString() : null;
                                // tmp.Price = sqlDr["tutar"].ToString() != "" ? Convert.ToDecimal(sqlDr["tutar"]) : 0;  
                                tmp.Price = sqlDr["tutar"].ToString() != "" ? decimal.Parse(sqlDr["tutar"].ToString(), CultureInfo.CurrentCulture) : 0;
                                tmp.Amount = sqlDr["siparisMiktari"].ToString() != "" ? Convert.ToInt64(sqlDr["siparisMiktari"]) : 0;
                                tmp.Barcode = sqlDr["Barcode"].ToString() != "" ? sqlDr["Barcode"].ToString() : null;
                                tmp.Renk = sqlDr["ColorName"].ToString() != "" ? sqlDr["ColorName"].ToString() : null;
                                tmp.Gender = sqlDr["Gender"].ToString() != "" ? (Gender)(int)sqlDr["Gender"] : 0;
                                tmp.ColorId = sqlDr["ColorId"].ToString() != "" ? Convert.ToInt64(sqlDr["ColorId"]) : 0;
                                tmp.ProductId = sqlDr["ProductId"].ToString() != "" ? Convert.ToInt64(sqlDr["ProductId"]) : 0;
                                tmp.SpId = sqlDr["ShippinbgId"].ToString() != "" ? Convert.ToInt64(sqlDr["ShippinbgId"]) : 0;
                                tmp.RenkBarcode = sqlDr["RenkBarcode"].ToString() != "" ? sqlDr["RenkBarcode"].ToString() : "";
                                SPPP.Add(tmp);
                            }
                        }

                    }
                    List<OrderShippingDto> listem = new List<OrderShippingDto>();
                    using (TexSoftContext db = new TexSoftContext())
                    {
                        foreach (var item in db.Products)
                        {
                            var productList = SPPP.Where(a => a.ProductId == item.Id);
                            if (productList.Count() > 0)
                            {
                                foreach (var item2 in productList)
                                {
                                    if (listem.Count == 0)
                                    {
                                        listem.Add(item2);
                                    }
                                    else
                                    {
                                        var check = listem.FirstOrDefault(a => a.ProductId == item2.ProductId && a.ColorId == item2.ColorId);
                                        if (check != null)
                                        {
                                            check.Amount = check.Amount + item2.Amount;
                                            check.Price = check.Price + item2.Price;
                                        }
                                        else
                                        {
                                            listem.Add(item2);
                                        }
                                    }
                                }
                            }


                        }
                    }
                    return listem;
                }
            }
            catch (Exception ex)
            {

                return null;
            }

        }

    }
}
