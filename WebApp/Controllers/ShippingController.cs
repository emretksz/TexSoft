using Application.Interface;
using AutoMapper;
using Core.Entities;
using Core.Utilities.Results;
using DataAccess.Interfaces;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
//using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using X.PagedList;
using static Entities.Enums.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebApp.Controllers
{
    [Authorize(Policy = "AdminMagazaDepo")]
    public class ShippingController : Controller
    {
        private readonly IShippingServices _shippingServices;
        private readonly IShippingDetailsServices _shippingDetailsServices;
        private readonly IColorServices _colorServices;
        private readonly ITempServices _tempServices;
        readonly IProductServices _productServices;
        readonly IStockServices _stockServices;
        readonly IMapper _mapper;
        private IHttpContextAccessor _httpContextAccessor;
        private readonly ITenantServices _tenantServices;
        readonly IAllStockListDal _allStockListDal;


        public ShippingController(IShippingServices shippingServices, IColorServices colorServices, IProductServices productServices, IStockServices stockServices, IShippingDetailsServices shippingDetailsServices, IMapper mapper, IHttpContextAccessor httpContextAccessor, ITenantServices tenantServices, ITempServices tempServices, IAllStockListDal allStockListDal)
        {
            _shippingServices = shippingServices;
            _colorServices = colorServices;
            _productServices = productServices;
            _stockServices = stockServices;
            _shippingDetailsServices = shippingDetailsServices;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _tenantServices = tenantServices;
            _tempServices = tempServices;
            _allStockListDal = allStockListDal;
        }

        //public async Task<IActionResult> ShippingIndex(int page = 1)
        //{///mapping sorunu

        //    var magazaMi = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;
        //    //User.Claims.FirstOrDefault(a=>a.)

        //    if (magazaMi.ToLower() == "magaza")
        //    {
        //        var res2 = await _shippingServices.IndexIcınGetirMagaza();
        //        ViewBag.Yetki = true;
        //        return View(res2.ToPagedList(page, 12));
        //    }
        //    else if (magazaMi.ToLower() == "admin")
        //    {
        //        ViewBag.Yetki = true;
        //    }
        //    else
        //    {
        //        ViewBag.Yetki = null;
        //    }
        //    var res = await _shippingServices.IndexIcınGetir();
        //    return View(res.ToPagedList(page, 12));

        //}
        public async Task<IActionResult> ShippingIndex(int page = 1)
        {///mapping sorunu

            var magazaMi = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;
            //User.Claims.FirstOrDefault(a=>a.)
            //if (magazaMi.ToLower() == "admin")
            //{
            //}
            //else
            //{
            //    ViewBag.Yetki = null;
            //}
            ViewBag.Yetki = true;
            if (magazaMi.ToLower() == "depo")
            {
                ViewBag.depoMu = true;
            }
            else
            {
                ViewBag.depoMu = null;
            }



            return View();

        }
        public async Task<IActionResult> ShippingIndexjson(string date)
        {///mapping sorunu

            var dates = new DateTime(Convert.ToInt32(date), 01, 01);
            var res = await _shippingServices.IndexIcınGetir(dates);
            var qq = JsonConvert.SerializeObject(res);
            return Json(qq);

        }
        public async Task<IActionResult> ChangePriceShipping(long shippingId, string newPrice)
        {///mapping sorunu

            try
            {
                if (String.IsNullOrEmpty(newPrice))
                {
                    return Json("Geçerli bir değer giriniz, Başarısız");
                }
                decimal values = decimal.Parse(newPrice, CultureInfo.CurrentCulture);
                var sp = await _shippingDetailsServices.GetById(shippingId);
                var shipping = await _shippingServices.GetById(sp.Data.ShippinbgId);

                ///sipariş ana fiyat güncellendi
                if (shipping.Data != null)
                {
                    string formattedString = shipping.Data.SiparisTutari;
                    decimal siparisTutari = decimal.Parse(formattedString, CultureInfo.CurrentCulture);
                    //if (siparisTutari > sp.Data.Price)
                    //{ }
                    var yeniSiparisTutari = (sp.Data.ShippingCount.Value * values);

                    siparisTutari = siparisTutari - sp.Data.Price; // eski tutar
                    siparisTutari = siparisTutari + yeniSiparisTutari; // yeni tutar

                    shipping.Data.SiparisTutari = siparisTutari.ToString();
                    await _shippingServices.UpdateAll(shipping.Data);
                }

                //sipariş detay fiyatı güncellenbdi
                sp.Data.UnitePrice = values;

                var res = await _shippingDetailsServices.UpdateAll(sp.Data);
                return Json("İşlem Başarılı");
            }
            catch (Exception)
            {
                return Json("işlem Başarısız");
            }

        }
        public async Task<IActionResult> UpdateFinishShippingListView(long spId)
        {

            if (spId != 0)
            {
                var result = await _tempServices.UpdateFinishShippingListView(spId);
                if (result == null || result.Count == 0)
                {
                    ViewBag.Uyari = true;
                    result = new List<ShippingProduct>()
                    {
                        new ShippingProduct()
                        {
                            ShippingId=0,
                            ShippingDetailsId=0
                        }
                    };
                }
                else
                {
                    ViewBag.Uyari = false;
                }
                return View("UpdateFinishShipping", result);
            }
            return BadRequest();
        }

        public async Task<IActionResult> ShippingDetailsUpdatePrice(long spId)
        {

            if (spId != 0)
            {
                var result = await _shippingDetailsServices.UpdateShippingPrice(spId);
                if (result == null || result.Count == 0)
                {
                    ViewBag.Uyari = true;
                    result = new List<ShippingDetails>()
                    {
                        new ShippingDetails()
                        {
                            ShippinbgId=0,
                        }
                    };
                }
                else
                {
                    ViewBag.Uyari = false;
                }
                return View("ShippingDetailsUpdatePrice", result);
            }
            return BadRequest();
        }

        public async Task<IActionResult> ShippingDetailsAddNewProduct(long spId)
        {

            if (spId != 0)
            {
                var result = await _shippingDetailsServices.GetShippingConfirmList(spId);

                ViewBag.Ids = spId;

                var urunler = await ProductUpdateForShipping(spId);
           
                //var xdd= urunler.GroupBy(a => a.ProductId).Distinct().ToList();
                ViewBag.Urun = urunler
                .GroupBy(u => u.ProductId)
                .Select(g => g.First())
                .ToList();
           var shipping = await _tempServices.GetParticalShipping(spId);
                if (shipping == null || shipping.Count==0)
                {

                    var liste = new List<ShippingProduct>();
                    liste.Add(new ShippingProduct());
                    ViewBag.Renkli = liste;
                }
                else
                {
                    ViewBag.Renkli = shipping;
                }



                return View("ShippingDetailsAddNewProduct", result);
            }
            return BadRequest();
        }
        public async Task<IActionResult> UruneEkle(long spId, long productId, long adet)
        {
            if (adet != 0)
            {
                var model = await _shippingDetailsServices.GetShippingDetailsForShippingId(spId, productId);

                var result = await _shippingDetailsServices.GetById(model.Id);

                result.Data.Amount = result.Data.Amount + adet;
                result.Data.ShippingCount += adet;
                await _shippingDetailsServices.UpdateAll(result.Data);


                var shipping = await _shippingServices.GetById(spId);

                shipping.Data.ShippingCount = (Convert.ToInt64(shipping.Data.ShippingCount) + adet).ToString();
                shipping.Data.IsComplated = false;

                await _shippingServices.UpdateAll(shipping.Data);

                return Json("ok");
            }

            return Json("-1");
        }

        public async Task<IActionResult> UrundenCikart(long spId, long productId, long adet, bool sil)
        {
            try
            {

                if (sil)
                {
                    //sipariş detayı silindi
                    var urun = await _shippingDetailsServices.GetShippingDetailsForShippingId(spId, productId);
                    await _shippingDetailsServices.Delete(urun);


                    //sipariş'den düşüldü  
                    var siparis = await _shippingServices.GetById(spId);
                    siparis.Data.ShippingCount = (Convert.ToInt64(siparis.Data.ShippingCount) - urun.ShippingCount).ToString();
                    if (Convert.ToInt64(siparis.Data.ShippingCount) < 0)
                    {
                        siparis.Data.ShippingCount = "0";
                    }

           
                    var prices2 = decimal.Parse(siparis.Data.SiparisTutari, CultureInfo.CurrentCulture);
                    siparis.Data.SiparisTutari = (prices2 - urun.Price).ToString();

                    await _shippingServices.UpdateAll(siparis.Data);



                    //temp'den silme
                    var temps = await _tempServices.RemoveShippingProduct(spId, productId);
                    foreach (var item in temps)
                    {
                        var stock = await _stockServices.GetStockAndProduct(item.ColorId, productId);
                        stock.StockCount += item.Count;
                        await _stockServices.UpdateAll(stock);

                        await _tempServices.Delete(item);

                        try
                        {
                            await _allStockListDal.CreateAsync(new()
                            {
                                ColorId = Convert.ToInt64(item.ColorId),
                                Count = item.Count,
                                ProductId = productId,
                                CreationTime = DateTime.UtcNow,
                                Code = "Stoğa eklendi, üründen çıkarma işlemi kullanıldı"
                            });
                        }
                        catch (Exception)
                        {

                        }
                    }

                }
                else if (adet != 0 && !sil)
                {
                    //var urunler = await ProductUpdateForShipping(spId);
                    var urunler = await _tempServices.GetParticalShipping(spId);
                    var renkVarMi = urunler.Where(a => a.ColorId != 0 && a.ColorName != "" && a.ColorName != null&&a.ProductId==productId).Count();
                    if (renkVarMi > 0)
                    {
                        return Json("-1");
                    }

                     /// yeni fiyatlandırma yapılmalı

                  
                    var model = await _shippingDetailsServices.GetShippingDetailsForShippingId(spId, productId);

                    var result = await _shippingDetailsServices.GetById(model.Id);

                    result.Data.Amount = result.Data.Amount - adet;
                    if (result.Data.Amount < 0)
                    {
                        result.Data.Amount = 0;
                    }
                    result.Data.ShippingCount -= adet;

                    if (result.Data.ShippingCount < 0)
                    {
                        result.Data.ShippingCount = 0;
                    }
                    await _shippingDetailsServices.UpdateAll(result.Data);

                    var shipping = await _shippingServices.GetById(spId);
                    shipping.Data.ShippingCount = (Convert.ToInt64(shipping.Data.ShippingCount) - adet).ToString();


     
                    await _shippingServices.UpdateAll(shipping.Data);


            

                    return Json("ok");
                }

            }
            catch (Exception)
            {

                return Json("-1");
            }
            return Json("ok");
        }

        public async Task<IActionResult> RenktenCikar(long spId, long productId, long adet, long colorId, bool sil)
        {

            if (sil)
            {
                var temps = await _tempServices.RemoveShippingProduct(spId, productId);
                foreach (var item in temps.Where(a=>a.ColorId==colorId).ToList())
                {

                    //sipariş'den düşüldü  
                    //var siparis = await _shippingServices.GetById(spId);
                    //siparis.Data.ShippingCount = (Convert.ToInt64(siparis.Data.ShippingCount) - adet).ToString();
                    //if (Convert.ToInt64(siparis.Data.ShippingCount) < 0)
                    //{
                    //    siparis.Data.ShippingCount = "0";
                    //}
                    //await _shippingServices.UpdateAll(siparis.Data);


                    //temp silindi
                    await _tempServices.Delete(item);


                    // stoklara eklendi
                    var stock = await _stockServices.GetStockAndProduct(item.ColorId, productId);
                    stock.StockCount += item.Count;
                    await _stockServices.UpdateAll(stock);

                      //sipariş detayı güncellendi
                    var model = await _shippingDetailsServices.GetShippingDetailsForShippingId(spId, productId);

                    var result = await _shippingDetailsServices.GetById(model.Id);
                    result.Data.Amount = result.Data.Amount + item.Count;

                    await _shippingDetailsServices.UpdateAll(result.Data);





                }
            }
            else if (adet != 0)
            {
                var model = await _shippingDetailsServices.GetShippingDetailsForShippingId(spId, productId);

                //sipariş detayı güncellendi

                var result = await _shippingDetailsServices.GetById(model.Id);

                result.Data.Amount = result.Data.Amount + adet; //  siparişten renk düşülürse toplam siparişte yer açmalıyım? burası hatalı olabilir ama bu şekilde yapıyorum
                if (result.Data.Amount == 0 || result.Data.Amount < 0)
                {
                    result.Data.Amount = 0;
                }
                //result.Data.ShippingCount -= adet;

                //if (result.Data.ShippingCount == 0 || result.Data.ShippingCount < 0)
                //{
                //    result.Data.ShippingCount = 0;
                //}
                await _shippingDetailsServices.UpdateAll(result.Data);


                //stok güncellendi

                var stock = await _stockServices.GetStockAndProduct(colorId, productId);
                stock.StockCount += adet;
                await _stockServices.UpdateAll(stock);

                try
                {
                    await _allStockListDal.CreateAsync(new()
                    {
                        ColorId = Convert.ToInt64(stock.ColorId),
                        Count = adet,
                        ProductId = productId,
                        CreationTime = DateTime.UtcNow,
                        Code = "Stoğa eklendi, RenktenCikar  işlemi kullanıldı"
                    });
                }
                catch (Exception)
                {

                }
                //sipariş  güncellendi
                var shipping = await _shippingServices.GetById(spId);

                shipping.Data.ShippingCount = (Convert.ToInt64(shipping.Data.ShippingCount) + adet).ToString(); // adet sipariş miktarına eklendi
                if (Convert.ToInt64(shipping.Data.ShippingCount) == 0 || Convert.ToInt64(shipping.Data.ShippingCount) < 0)
                {
                    shipping.Data.ShippingCount = "0";
                }

                await _shippingServices.UpdateAll(shipping.Data);



                var temp = await _tempServices.RemoveShippingAmount(spId, colorId, productId);

                temp.Count = temp.Count - adet;
                if (temp.Count < 0)
                {
                    temp.Count = 0;
                }
                temp.IsComplated = false;

                await _tempServices.UpdateAll(temp);


                return Json("ok");
            }

            return Json("-1");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateFinishShippingPost([FromBody] List<ShippingProduct> shippingProduct)
        {
            if (shippingProduct.Count == 0)
            {
                return Json(false);
            }
            var result = await _tempServices.UpdateFinishShippingPost(shippingProduct.Where(item => item != null).ToList());
            return Json(result);
        }


        public async Task<IActionResult> Create()
        {
            ViewBag.Enums = GetEnumList(1);
            ViewBag.Age = GetEnumList(2);
            ViewBag.Gender = GetEnumList(3);
            Shippings shippings = new Shippings();
            var result = await _tenantServices.GetAll();
            ViewBag.Tenants = result.Data;
            var productList = await _productServices.GetAll();
            ViewBag.Product = productList.Data.OrderBy(a => a.ModelCode).OrderBy(a => a.Age).ToList();
            return View(shippings);

        }



        SelectList GetEnumList(int type)
        {
            switch (type)
            {
                case 1:
                    var list = from ShippingStasus e in Enum.GetValues(typeof(ShippingStasus))
                               select new { EnumId = (int)e, Status = e.ToString() };
                    return new SelectList(list, "EnumId", "Status", 1);

                case 2:
                    var list2 = from Age e in Enum.GetValues(typeof(Age))
                                select new { EnumId = (int)e, Age = e.ToString() };
                    return new SelectList(list2, "EnumId", "Age", 1);
                case 3:
                    var list3 = from Gender e in Enum.GetValues(typeof(Gender))
                                select new { EnumId = (int)e, Gender = e.ToString() };
                    return new SelectList(list3, "EnumId", "Gender", 1);
                default:
                    return null;
            }


        }
        [HttpPost]
        public async Task<IActionResult> Create(Shippings shippings)
        {
            if (HttpContext.Session.GetString("shippingId") != "" || HttpContext.Session.GetString("shippingId") != null)
                HttpContext.Session.Clear();

            //if (shippings.MagazaMi != null)
            //{
            //    if (shippings.MagazaMi.Value)
            //    {
            //        if (HttpContext.Session.GetString("magazaMi") != "" || HttpContext.Session.GetString("magazaMi") != null)
            //            HttpContext.Session.Clear();
            //        HttpContext.Session.SetString("magazaMi", "magaza");
            //    }
            //    else
            //    {
            //        shippings.MagazaMi = null;
            //    }

            //}
            shippings.IsActive = true;
            shippings.TenantId = shippings.TenantId;
            ////var product = await _productServices.GetById(shippings.ProductId.Value);
            //shippings.Age = product.Data.Age;
            //shippings.Gender = product.Data.Gender;
            var ids = await _shippingServices.Add(shippings);
            //ProductColorDto dto = new ProductColorDto();
            ViewBag.Ids = ids;
            //dto.Products = (await _productServices.GetAll()).Data;
            //dto.ProductColor = (await _colorServices.GetAll()).Data;
            HttpContext.Session.SetString("shippingId", ids.ToString());

            return RedirectToAction("NewShippingView", new { id = ids });
        }
        public async Task<IActionResult> NewShippingView(long id)
        {

            ViewBag.Ids = id;
            return View();
        }

        //public async Task<IActionResult> AddShippingDetail(long id)
        //{

        //}
        public async Task<List<ShippingProduct>> ProductUpdateForShipping(long shippingId2)
        {

            PagedListShippingProduct dto = new PagedListShippingProduct();
            var shippingId = shippingId2;
            var ids = Convert.ToInt64(shippingId);
            ViewBag.Ids = ids;
            List<ShippingDetails> siparisListesi = new List<ShippingDetails>();

            if (ids != null && ids != 0)
            {
                var query = await _shippingDetailsServices.GetAllColorAndProduct(ids);
                siparisListesi = query;

            }
            List<ShippingProduct> shippingList = new List<ShippingProduct>();
            foreach (var item in siparisListesi)
            {
                ShippingProduct shipping = new ShippingProduct();
                shipping.ProductId = item.ProductId;
                if (item.ProductId != 0)
                {
                    var productName = await _productServices.GetProductIdByAge(item.ProductId);
                    shipping.ProductName = productName.ModelName;
                    shipping.ModelCode = productName.ModelCode;
                    shipping.Age = productName.ProductAges.Name;
                }
                shipping.ShippingId = item.ShippinbgId;
                if (item.ColorId.HasValue)
                {
                    shipping.ColorId = item.ColorId.Value;
                }
           

                if (item.ColorId.HasValue&& item.ColorId != 0)
                {
                    var colorName = await _colorServices.GetById(item.ColorId.Value);
                    shipping.ColorName = colorName.Data.ColorName;
                }
                shipping.Count = item.ShippingCount.Value;
                shippingList.Add(shipping);
            }
            var spList = shippingList;

            return spList;
        }
        public async Task<IActionResult> AddShippingDetail(long shippingId, int? page = 1)
        {


            PagedListShippingProduct dto = new PagedListShippingProduct();
            //var shippingId = HttpContext.Session.GetString("shippingId");
            ViewBag.Ids = shippingId;
            var produtList = await _productServices.GetAll();
            var deneme = produtList.Data.ToPagedList(page.Value, 3);
            //dto.Products =
            ViewBag.Product = produtList.Data.Count;
            //dto.pageCount = 
            //var colorList = await _colorServices.GetAll();
            //dto.ProductColor = colorList.Data.ToPagedList(color.Value, 5);
            //List<ShippingDetails> siparisListesi = new List<ShippingDetails>();

            //if (ids != null && ids != 0)
            //{
            //    var query = await _shippingDetailsServices.GetAll(a => a.ShippinbgId == ids);
            //    siparisListesi = query.Data;

            //}
            //List<ShippingProduct> shippingList = new List<ShippingProduct>();
            //foreach (var item in siparisListesi)
            //{
            //    ShippingProduct shipping = new ShippingProduct();
            //    shipping.ProductId = item.ProductId;
            //    if (item.ProductId != 0)
            //    {
            //        var productName = await _productServices.GetById(item.ProductId);
            //        shipping.ProductName = productName.Data.ModelName;
            //    }
            //    shipping.ShippingId = item.ShippinbgId;
            //    shipping.ColorId = item.ColorId;

            //    if (item.ColorId != 0)
            //    {
            //        var colorName = await _colorServices.GetById(item.ColorId);
            //        shipping.ColorName = colorName.Data.ColorName;
            //    }
            //    shipping.Count = item.ShippingCount;
            //    shippingList.Add(shipping);
            //}
            //dto.ShippingProduct = shippingList;

            return View(deneme);
        }
        public async Task<IActionResult> ShippingComplated()
        {

            PagedListShippingProduct dto = new PagedListShippingProduct();
            var shippingId = HttpContext.Session.GetString("shippingId");
            var ids = Convert.ToInt64(shippingId);
            ViewBag.Ids = ids;
            List<ShippingDetails> siparisListesi = new List<ShippingDetails>();

            if (ids != null && ids != 0)
            {
                var query = await _shippingDetailsServices.GetAll(a => a.ShippinbgId == ids);
                siparisListesi = query.Data;

            }
            List<ShippingProduct> shippingList = new List<ShippingProduct>();
            foreach (var item in siparisListesi)
            {
                ShippingProduct shipping = new ShippingProduct();
                shipping.ProductId = item.ProductId;
                if (item.ProductId != 0)
                {
                    var productName = await _productServices.GetById(item.ProductId);
                    shipping.ProductName = productName.Data.ModelName;
                }
                shipping.ShippingId = item.ShippinbgId;
                shipping.ColorId = item.ColorId.Value;

                if (item.ColorId != 0)
                {
                    var colorName = await _colorServices.GetById(item.ColorId.Value);
                    shipping.ColorName = colorName.Data.ColorName;
                }
                shipping.Count = item.ShippingCount.Value;
                shippingList.Add(shipping);
            }
            var spList = shippingList;

            return PartialView("ShippingComplated", spList);
        }

        public async Task<IActionResult> AddShippingDetailsColorPartical(int? color = 1)
        {
            PagedListShippingProduct dto = new PagedListShippingProduct();
            var colorList = await _colorServices.GetAll();
            var query = colorList.Data.ToPagedList(color.Value, 12);
            //var qq = colorList.Data.ToPagedList(color.Value, 5);
            ViewBag.Count = query.Count;
            ViewBag.ColorCount = colorList.Data.Count + 2;
            //List<ShippingDetails> siparisListesi = new List<ShippingDetails>();
            //List<ShippingProduct> shippingList = new List<ShippingProduct>();
            //foreach (var item in siparisListesi)
            //{
            //    ShippingProduct shipping = new ShippingProduct();
            //    shipping.ColorId = item.ColorId;
            //    if (item.ColorId != 0)
            //    {
            //        var colorName = await _colorServices.GetById(item.ColorId);
            //        shipping.ColorName = colorName.Data.ColorName;
            //    }
            //    shipping.Count = item.ShippingCount;
            //    shippingList.Add(shipping);
            //}

            //dto.ShippingProduct = shippingList;
            return PartialView("AddShippingDetailsColorPartical", query);
        }


        [HttpPost]
        public async Task<bool> AddShippingDetails(long ShippingId, long productId, String body, bool refresh)
        {
            var model = JsonConvert.DeserializeObject<List<ColorDto>>(body);

            foreach (var item in model)
            {
                ShippingDetails shippingDetails = new()
                {
                    ShippinbgId = ShippingId,
                    ProductId = productId,
                };
                item.ColorName = item.ColorName.Replace(",", "");
                item.Count = item.Count.Replace(",", "");
                if (item.ColorName != "" && item.Count != "" && item.ColorName != "," && item.Count != "," && item.Count != "on" && item.ColorName != "on")
                {

                    shippingDetails.ColorId = Convert.ToInt64(item.ColorName);
                    shippingDetails.ShippingCount = Convert.ToInt64(item.Count);

                    var check = await _shippingDetailsServices.GetAll(a => a.ProductId == productId && a.ColorId == Convert.ToInt64(item.ColorName) && a.ShippinbgId == ShippingId);
                    if (check.Data.Count > 0)
                    {
                        var query = check.Data.FirstOrDefault();
                        query.ShippingCount = query.ShippingCount + Convert.ToInt64(item.Count);
                        await _shippingDetailsServices.UpdateAll(query);

                        var shippings = await _shippingServices.GetById(ShippingId);
                        shippings.Data.ShippingCount = (Convert.ToInt64(shippings.Data.ShippingCount) - Convert.ToInt64(item.Count)).ToString();
                        await _shippingServices.UpdateAll(shippings.Data);
                    }
                    else
                    {

                        await _shippingDetailsServices.Add(shippingDetails);
                    }
                }
            }
            return true;

        }

        [HttpPost]
        public async Task<bool> UpdateShippingDetails(ShippingDetails shippingDetail)
        {

            var result = await _shippingDetailsServices.GetAll(a => a.ShippinbgId == shippingDetail.ShippinbgId && a.ProductId == shippingDetail.ProductId && a.ColorId == shippingDetail.ColorId);
            ShippingDetails shippingDetails = new()
            {
                Id = result.Data.FirstOrDefault().Id,
                ShippinbgId = shippingDetail.ShippinbgId,
                ProductId = shippingDetail.ProductId,
                ColorId = shippingDetail.ColorId,
                ShippingCount = shippingDetail.ShippingCount,
            };

            await _shippingDetailsServices.UpdateAll(shippingDetails);
            return true;
        }

        public async Task<bool> DeleteShippinDetails(ShippingDetails shippingDetail)
        {

            var result = await _shippingDetailsServices.GetAll(a => a.ShippinbgId == shippingDetail.ShippinbgId && a.ProductId == shippingDetail.ProductId && a.ColorId == shippingDetail.ColorId);
            foreach (var item in result.Data)
            {
                await _shippingDetailsServices.Delete(item);
            }
            return true;
        }
        public async Task<IActionResult> DeleteShipping(ShippingDetails shippingDetail)
        {
            var result = await _shippingDetailsServices.GetAll(a => a.ShippinbgId == shippingDetail.ShippinbgId);
            foreach (var item in result.Data)
            {
                await _shippingDetailsServices.Delete(item);

            }
            var tempresult = await _tempServices.GetAll(a => a.ShippigId == shippingDetail.ShippinbgId);
            foreach (var item in tempresult.Data)
            {
                await _tempServices.Delete(item);
            }
            var deleteShipping = await _shippingServices.GetById(shippingDetail.ShippinbgId);
            await _shippingServices.Delete(deleteShipping.Data);
            return RedirectToAction("ShippingIndex");
        }

        public async Task<IActionResult> ComplatedShippin(long ShippinbgId)
        {
            var result = await _shippingServices.GetById(ShippinbgId);
            var list = await _shippingDetailsServices.GetAll(a => a.ShippinbgId == ShippinbgId);
            long stockCount = 0;
            foreach (var item in list.Data)
            {
                stockCount += item.ShippingCount.Value;
            }
            result.Data.IsComplated = true;
            result.Data.ShippingCount = stockCount.ToString();
            await _shippingServices.UpdateAll(result.Data);
            return RedirectToAction("ShippingIndex");
        }


        public async Task<IActionResult> ShippingDetails(long ShippinbgId)
        {

            var result = await _shippingServices.IndexShipping(ShippinbgId);

            return View(result);
        }
        public async Task<IActionResult> ColorAndCount(long ShippinbgId, long productId)
        {

            var result = await _shippingServices.SiparisİicndekiRenkveMiktarlari(ShippinbgId, productId);

            return Json(result);
        }
        public async Task<IActionResult> ModalFadePartical(long ShippinbgId, long productId)
        {
            var result = await _shippingServices.SiparisİicndekiRenkveMiktarlari(ShippinbgId, productId);
            return PartialView("ModelFadeShipping", result);
        }
        //public async Task<IActionResult> NewShippingViewPartical(SearchParticalDto p)
        //{
        //    var model = await _productServices.GetFilterProduct(p, 1, 12);
        //    var models = await model.ToListAsync();
        //    return PartialView("NewShippingViewPartical", models);
        //}

        public async Task<IActionResult> NewShippingViewPartical(SearchParticalDto p)
        {
            // Ensure p is not null
            p = p ?? new SearchParticalDto();

            var model = await _productServices.GetFilterProductPageFilter(p, p.page, p.pageSize);
            var models = await model.ToListAsync();

            ViewBag.CurrentPage = p.page;
            ViewBag.PageSize = p.pageSize;
            ViewBag.TotalItems = await _productServices.GetFilterProductPageFilterCount(p, p.page, p.pageSize); ; 

            return PartialView("NewShippingViewPartical", models);
        }
   

        
        //public async Task<IActionResult> ShippingCofirmList([FromBody] List<ConvertShippngDto> convertShippngDto)
        //{

        //    try
        //    {
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }

        //}
        [HttpPost]
        public async Task<IActionResult> ShippingCofirmList2([FromBody] List<ConvertShippngDto> convertShippngDto)
        {

            try
            {
                long shippingId = 0;
                // var model = JsonConvert.DeserializeObject<List<ConvertShippngDto>>(body);

                
                var query = convertShippngDto.FirstOrDefault(a => a != null && a.shippinbgId != null && a.shippinbgId != "");
                if (query != null)
                {
                    shippingId = Convert.ToInt64(query.shippinbgId);
                }
                List<ShippingConfirmListDto> models = new List<ShippingConfirmListDto>();

                foreach (var item in convertShippngDto)
                {
                    if (item != null && item.price.ToString() != "" && item.amount.ToString() != "")
                    {
                        decimal values = decimal.Parse(item.price, CultureInfo.InvariantCulture);

                        decimal unitepriceValue = decimal.Parse(item.uniteprice, CultureInfo.InvariantCulture);
                        ShippingDetails sp = new ShippingDetails();
                        sp.ProductId = Convert.ToInt64(item.productId);
                        sp.Price = values;//Convert.ToDecimal(item.price);
                        sp.Amount = Convert.ToInt64(item.amount);
                        sp.ShippinbgId = Convert.ToInt64(item.shippinbgId);
                        sp.ShippingCount = Convert.ToInt64(item.amount);
                        sp.UnitePrice = unitepriceValue;


                        /// aynı ürün ekleniyorsa adet güncelle //fiyat güncelle  yoksa direkt ekle
                        /// 

                        var bulunanSiparis= await _shippingDetailsServices.GetShippingConfirmList(shippingId);
                        bool success = false;
                        if (bulunanSiparis!=null && bulunanSiparis.Count>0)
                        {
                           var siparisiGuncelle =  bulunanSiparis.FirstOrDefault(a => a.ProductId == sp.ProductId);
                            if (siparisiGuncelle!=null)
                            {
                                sp.Amount = siparisiGuncelle.Amount+sp.Amount;
                                sp.ShippingCount = siparisiGuncelle.Amount + Convert.ToInt64(item.amount);

                                sp.Id = siparisiGuncelle.DetailsId;
                                await _shippingDetailsServices.UpdateAll(sp);
                                success = true;

                            }
                            else
                            {
                                var add = await _shippingDetailsServices.Add(sp);
                                if (add.Success)
                                    success = true;
                            }
                        }
                        else
                        {
                            var add = await _shippingDetailsServices.Add(sp);
                            if (add.Success)
                                success = true;
                        }
                       
                        if (success)
                        {
                            var update = await _shippingServices.GetById(Convert.ToInt64(item.shippinbgId));
                            update.Data.ShippingCount = (Convert.ToInt64(update.Data.ShippingCount) + Convert.ToInt64(item.amount)).ToString();

                            var prices = update.Data.SiparisTutari != null ? update.Data.SiparisTutari : "0";
                            var prices2 = decimal.Parse(prices, CultureInfo.CurrentCulture);
                            var resq = values + prices2;

                            update.Data.SiparisTutari = resq.ToString();

                            update.Data.IsComplated = false;

                            //update.Data.SiparisTutari = (Convert.ToDecimal(update.Data.SiparisTutari) + Convert.ToDecimal(item.price)).ToString();


                            await _shippingServices.UpdateAll(update.Data);
                        }
                        shippingId = Convert.ToInt64(item.shippinbgId);
                    }

                }
                models = await _shippingDetailsServices.GetShippingConfirmList(shippingId);
                return PartialView("ShippingConfirmList", models);
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        public async Task<IActionResult> CheckShippingCofirmListReturn(long spId)
        {
            var models = await _shippingDetailsServices.GetShippingConfirmList(spId);
            if (models != null && models.Count > 0)
            {
                return PartialView("ShippingConfirmList", models);
            }
            return Json("");
        }

        public async Task<IActionResult> ShippingCofirmListReturn(/*[FromBody] List<ConvertShippngDto> convertShippngDto,*//* String body,*//*ShippingDetails shippingDetails*/ bool? reload = false, long spId = 0)
        {

            try
            {
                // var model = JsonConvert.DeserializeObject<List<ConvertShippngDto>>(body);
                //var model = new List<ConvertShippngDto>();
                //if (reload.HasValue)
                //{
                //    if (!reload.Value)
                //    {
                //        List<ShippingConfirmListDto> models = new List<ShippingConfirmListDto>();
                //        long shippingId = 0;
                //        foreach (var item in model)
                //        {
                //            ShippingDetails sp = new ShippingDetails();
                //            sp.ProductId = Convert.ToInt64(item.productId);
                //            sp.Price = Convert.ToDecimal(item.price);
                //            sp.Amount = Convert.ToInt64(item.amount);
                //            sp.ShippinbgId = Convert.ToInt64(item.shippinbgId);
                //            var add = await _shippingDetailsServices.Add(sp);
                //            shippingId = Convert.ToInt64(item.shippinbgId); ;
                //        }
                //        models = await _shippingDetailsServices.GetShippingConfirmList(shippingId);
                //        return PartialView("ShippingConfirmList", models);
                //    }
                //}
                if (spId != 0)
                {
                    var models = await _shippingDetailsServices.GetShippingConfirmList(spId);
                    return PartialView("ShippingConfirmList", models);
                }
                return null;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        public async Task<bool> DeleteShippingDetails(long spDetailsId)
        {
            var model = await _shippingDetailsServices.GetById(spDetailsId);
            if (model != null)
            {
                ///modeol silerken temp'de varsa silmesin

                var check = await _tempServices.GetParticalShipping(model.Data.ShippinbgId);
                if (check != null && check.Count != 0)
                {
                    if (check.Where(a => a.ProductId == model.Data.ProductId).Count() > 0)
                    {
                        return false;
                    }
                }
            }
            await _shippingDetailsServices.Delete(model.Data);

            /// detay silinice sipariş miktarından düş
            var update = await _shippingServices.GetById(Convert.ToInt64(model.Data.ShippinbgId));
            update.Data.ShippingCount = (Convert.ToInt64(update.Data.ShippingCount) - Convert.ToInt64(model.Data.Amount)).ToString();

            var prices = update.Data.SiparisTutari != null ? update.Data.SiparisTutari : "0";
            var prices2 = decimal.Parse(prices, CultureInfo.CurrentCulture);

            var priceData = decimal.Parse(model.Data.Price.ToString(), CultureInfo.CurrentCulture);
            var resq = prices2 - priceData;

            //update.Data.SiparisTutari = (Convert.ToDecimal(update.Data.SiparisTutari) - Convert.ToDecimal(model.Data.Price)).ToString();
            update.Data.SiparisTutari = resq.ToString();
            update.Data.IsComplated = false;
            update.Data.ShippingStasus = ShippingStasus.Acil;
            await _shippingServices.UpdateAll(update.Data);



            //update.Data.SiparisTutari = resq.ToString();


            return true;
        }


        //  public IActionResult UpdateShippigVC(long shippingId,long productId,long colorId,long count ) => ViewComponent("ShippingViewComponent", new { shippingId, productId, colorId, count });

        //public IActionResult UpdateShippigVC(long shippingId, long productId, long colorId, long count)
        //{
        //    return ViewComponent("ShippingViewComponent", new { shippingId, productId, colorId, count });
        //}
    }
}
