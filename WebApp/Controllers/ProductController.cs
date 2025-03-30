using Application.Interface;
using AutoMapper;
using Entities.Concrete;
using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static Entities.Enums.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.CodeAnalysis;
//using PagedList;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static System.Net.WebRequestMethods;
//using PagedList.Core.Mvc;
using X.PagedList.Mvc.Core;
using X.PagedList.Mvc;
using X.PagedList;
using Core.Entities;
using Core.Utilities.Results;
using System.Security.Cryptography.Xml;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Controllers
{
    //[Area("Product")]
    [Authorize(Policy = "AdminMagazaDepo")]
    public class ProductController : Controller
    {

        readonly IProductServices _productServices;
        readonly IProductDal _productRepository;
        readonly IColorServices _colorServices;
        readonly IStockServices _stockServices;
        readonly ITeklilerServices _teklilerServices;
        private readonly IWebHostEnvironment _evn;
        private readonly IMagazaStock _stockRepository;
        readonly IMapper _mapper;
        readonly IProductAgeDal _ageRepository;
        readonly IAllStockListDal _allStockListDal;

        public ProductController(IProductServices productServices, IStockServices stockServices, IMapper mapper, IColorServices colorServices, ITeklilerServices teklilerServices, IWebHostEnvironment evn, IMagazaStock stockRepository, IProductAgeDal ageRepository, IAllStockListDal allStockListDal)
        {
            _mapper = mapper;
            _productServices = productServices;
            _stockServices = stockServices;
            _colorServices = colorServices;
            _teklilerServices = teklilerServices;
            _evn = evn;
            _stockRepository = stockRepository;
            _ageRepository = ageRepository;
            _allStockListDal = allStockListDal;
        }
        public async Task<IActionResult> ProductIndex(int page =1, SearchParticalDto p=null)
        {
            //   var model = await _productServices.GetAll();
            //= new SearchParticalDto();
            ViewBag.search = false;
            var model = await _productServices.GetFilterProduct(p, page, 12);
            List<Product> result = new List<Product>();
            return View(await model.ToPagedListAsync(page,12)/*Data.ToPagedList(page,12)*/);
        }
        public async Task<IActionResult> ProductPartical(int page = 1, SearchParticalDto p = null)
        {
            var model = await _productServices.GetFilterProduct(p, page,12);
            IPagedList<Product> productItems;
            if (p.Search)
            {
                productItems = await model.ToPagedListAsync(1,1000);
                ViewBag.search = true;
            }
            else
            {
                 productItems = model.ToPagedList(page,5);
                ViewBag.search = false; 
            }
            ViewBag.pageCount = productItems.Count;
            return PartialView("ProductParticalView", productItems);
        }

        public async Task<IActionResult> ProductAdd(CreateProductDto createProductDto, IFormFile file)
        {
            if (file != null)
            {
                var FileDic = "images";
                string FilePath = Path.Combine(_evn.WebRootPath, FileDic);
                if (!Directory.Exists(FilePath))

                    Directory.CreateDirectory(FilePath);
                Guid g = Guid.NewGuid();
                var fileName = g;
                var filePath = Path.Combine(FilePath, fileName.ToString() + ".png");

                createProductDto.ModelImageUrl = "/images/" + fileName + ".png";

                using (FileStream fs = System.IO.File.Create(filePath))
                {
                    file.CopyTo(fs);
                }
            }

            var productId = await _productServices.Add(_mapper.Map<Product>(createProductDto));
    



            return RedirectToAction("AddColorForProduct",new { productId = productId });
        }
        public async Task<IActionResult> AddColorForProduct(long productId)
        {
            ProductColorDto dto = new ProductColorDto();
            // var obje = await _productServices.GetById(productId);
            dto.SingleProduct = await _productServices.GetProductIdByAge(productId);
            dto.ProductColor = ((await _colorServices.GetAll()).Data);
            ViewBag.ProductId = productId;
            var colorList = await _colorServices.GetAll();
            ViewBag.ColorCount = colorList.Data.Count;
            return View(dto);
        }

        SelectList GetEnumList()
        {
            var list = from Gender e in Enum.GetValues(typeof(Gender))
                       select new { EnumId = (int)e, Gender = e.ToString() };
            return new SelectList(list, "EnumId", "Gender", 1);

        }
        public async Task<IActionResult> ProductColorList(int page =1)
        {
            var result = await _colorServices.GetAll();
            ViewBag.ColorCount = result.Data.Count;
            var colors = result.Data.ToPagedList(page,12);
            return PartialView("ProductColorList", colors);
        }
        public async Task<IActionResult> UpdateProductStockColor(int page = 1)
        {
            var result = await _colorServices.GetAll();
            var colorList = await _colorServices.GetAll();
            ViewBag.ColorCountForStock = result.Data.Count;
            var colors = colorList.Data.ToPagedList(page, 12);
            return PartialView("UpdateProductStockColor", colors);
        }

        public async Task<IActionResult> UpdateProduct(long id)
        {
            var product = await _productServices.GetByProductQuery(id);
            ViewBag.productAge = new SelectList(((await _ageRepository.GetAllAsync())), "Id", "Name");
            ViewBag.Gender = GetEnumListGender();
            return View("UpdateProduct", product);
        }
        SelectList GetEnumListGender()
        {
            var list = from Gender e in Enum.GetValues(typeof(Gender))
                       select new { EnumId = (int)e, Gender = e.ToString() };
            return new SelectList(list, "EnumId", "Gender", 1);

        }
        [HttpPost]
        public async Task<IActionResult> UpdateProduct(CreateProductDto createProductDto, IFormFile file)
        {
            var old = await _productServices.GetById(createProductDto.ProductId);
            if (file != null)
            {
                var FileDic = "images";
                string FilePath = Path.Combine(_evn.WebRootPath, FileDic);
                if (!Directory.Exists(FilePath))
                    Directory.CreateDirectory(FilePath);
                Guid g = Guid.NewGuid();
                var fileName = g;
                var split = file.FileName.Split('.');

                var filePath = Path.Combine(FilePath, split[0]+fileName + ".png");

                createProductDto.ModelImageUrl = "/images/" + split[0] + fileName + ".png";
                if (old.Data.ModelImageUrl!=null)
                {
                    var filePathdelte = Path.Combine(FilePath, old.Data.ModelImageUrl);
                    if (System.IO.Directory.Exists(filePathdelte))
                    {
                        System.IO.Directory.Delete(filePathdelte);
                    }
                }

                using (FileStream fs = System.IO.File.Create(filePath))
                {
                    file.CopyTo(fs);
                }
            }
            else
            {
                createProductDto.ModelImageUrl = old.Data.ModelImageUrl;
            }
            var product = _mapper.Map<Product>(createProductDto);
            product.Id = createProductDto.ProductId;
            ProductAges age = new ProductAges();
            age.Id = createProductDto.ProductAgesId;
            age.Name = "";
            product.ProductAges = age;

            var result = await _productServices.UpdateAll(product);
            return RedirectToAction("ProductIndex"/*,(await _productServices.GetAll()).Data*/);
        }
        [HttpPost]
        public async Task<IActionResult> AddColorForProduct(long productId, string body, bool? update = null, string date = null)
        {
            var colors = JsonConvert.DeserializeObject<List<ColorDto>>(body)
                .Where(c => IsValidColorInput(c.ColorName, c.Count))
                .Select(c => new ColorDto
                {
                    ColorName = c.ColorName.Replace(",", ""),
                    Count = c.Count.Replace(",", ""),
                    Barcode = c.Barcode
                });

            foreach (var color in colors)
            {
                if (update == true)
                {
                    await HandleStockUpdate(productId, color, date);
                }
                else
                {
                    await HandleStockAddition(productId, color, date);
                }
            }

            return RedirectToAction("ProductIndex");
        }

        private bool IsValidColorInput(string colorName, string count) =>
            !string.IsNullOrEmpty(colorName) &&
            !string.IsNullOrEmpty(count) &&
            colorName != "," &&
            count != "," &&
            count != "on" &&
            colorName != "on";

        private async Task HandleStockUpdate(long productId, ColorDto color, string date)
        {
            var existingStock = (await _stockServices.GetAll(s =>
                s.ProductId == productId &&
                s.ColorId == Convert.ToInt64(color.ColorName) &&
                s.StockYear == date))
                .Data.FirstOrDefault();

            var barcode = string.IsNullOrEmpty(color.Barcode) ? existingStock?.RenkBarcode : color.Barcode;
            var count = Convert.ToInt64(color.Count);

            if (existingStock == null)
            {
                await AddNewStock(productId, color, date, count, barcode);
            }
            else
            {
                await UpdateExistingStock(productId, existingStock, color, date, count, barcode);
            }
        }

        private async Task HandleStockAddition(long productId, ColorDto color, string date)
        {
            var existingStock = (await _stockServices.GetAll(s =>
                s.ProductId == productId &&
                s.ColorId == Convert.ToInt64(color.ColorName) &&
                s.tekliId == null &&
                s.StockYear == date))
                .Data.FirstOrDefault();

            var barcode = string.IsNullOrEmpty(color.Barcode) ? existingStock?.RenkBarcode : color.Barcode;
            var count = Convert.ToInt64(color.Count);
            var stockTemp = existingStock != null ? existingStock.StockTemp + count : count;

            await _stockServices.Add(new()
            {
                ProductId = productId,
                ColorId = Convert.ToInt64(color.ColorName),
                StockCount = count,
                RegisterDate = DateTime.UtcNow.ToString(),
                StockStatus = StockStatus.UrunEklendi,
                MagazaMi = null,
                StockTemp = stockTemp,
                StockYear = date,
                RenkBarcode = barcode ?? ""
            });

            await LogStockAction(productId, color.ColorName, count, "ürünün ilk stogu");
        }

        private async Task AddNewStock(long productId, ColorDto color, string date, long count, string barcode)
        {
            await _stockServices.Add(new()
            {
                ProductId = productId,
                ColorId = Convert.ToInt64(color.ColorName),
                StockCount = count,
                RegisterDate = DateTime.UtcNow.ToString(),
                StockStatus = StockStatus.UrunEklendi,
                MagazaMi = null,
                StockTemp = count,
                StockYear = date,
                RenkBarcode = barcode
            });

            await LogStockAction(productId, color.ColorName, count, "Stock Eklendi");
        }

        private async Task UpdateExistingStock(long productId, Stock existingStock, ColorDto color, string date, long count, string barcode)
        {
            long stockDifference = existingStock.StockCount - count;
            long absoluteDifference = Convert.ToInt64(Math.Abs(stockDifference).ToString());
            long newStockTemp;

            if (existingStock.StockCount > count)
            {
                newStockTemp = existingStock.StockTemp - absoluteDifference;
                await LogStockAdjustment(productId, color.ColorName, count, stockDifference);
            }
            else
            {
                newStockTemp = existingStock.StockTemp + absoluteDifference;
                await LogStockAction(productId, color.ColorName, count, "stock güncellendi , yeni stok");
            }

            await _stockServices.UpdateAll(new()
            {
                Id = existingStock.Id,
                ProductId = productId,
                ColorId = Convert.ToInt64(color.ColorName),
                StockTemp = newStockTemp,
                StockCount = count != 0 ? count : existingStock.StockCount,
                RegisterDate = DateTime.UtcNow.ToString(),
                StockStatus = StockStatus.UrunEklendi,
                MagazaMi = null,
                StockYear = date,
                RenkBarcode = barcode
            });
        }

        private async Task LogStockAction(long productId, string colorId, long count, string message)
        {
            try
            {
                await _allStockListDal.CreateAsync(new()
                {
                    ColorId = Convert.ToInt64(colorId),
                    Count = count,
                    ProductId = productId,
                    CreationTime = DateTime.UtcNow,
                    Code = message
                });
            }
            catch
            {
                // Consider proper error handling/logging here
            }
        }

        private async Task LogStockAdjustment(long productId, string colorId, long count, long difference)
        {
            await LogStockAction(productId, colorId, count, "Eklenen stok var olandan az,stok sayısı düşürüldü");
            await LogStockAction(productId, colorId, -difference, "Eklenen stok var olandan az,stok sayısı düşürüldü");
        }
        public async Task<IActionResult> GetAllProductAndStock(long id)
        {
            //var result = await _productServices.GetProductStock(id);
           ViewBag.ProductId = id;
            return View();
        }
        public async Task<IActionResult> GetAllProductAndStockJson(long id)
        {
            var result = await _productServices.GetProductStock(id);
            var models = JsonConvert.SerializeObject(result);
            return Json(models);
        }
        public async Task<IActionResult> SearchPartical(long id)
        {

           ViewBag.AgeList = await _ageRepository.GetAllAsync();
            return PartialView("SearchPartical");
        }
        public async Task<IActionResult> SearchParticalL(SearchParticalDto p)
        {
            var model = await _productServices.GetAll();
            return PartialView("SearchPartical");
        }
        
        public async Task<IActionResult> Delete(long id)
        {
            var product =await _productServices.GetById(id);
            var result = await _productServices.Delete(product.Data);
            return RedirectToAction("ProductIndex");
        }
        public async Task<IActionResult> ProductStockAdd(ProductStockAdd productStockAdd)
        {

            return View("ProductIndex", (await _productServices.GetAll()).Data);
        }
        public async Task<IActionResult> UpdateProductStock(long productId)
        {
            
            ProductColorDto dto = new ProductColorDto();
            var result = await _productServices.GetProductStockUpdate(productId);
            dto.Products = result;
            dto.ProductColor = (await _colorServices.GetAll()).Data;
            ViewBag.ProductId = productId;
            ViewBag.ColorCount = result.Count;
            return View(dto);
        }
        public async Task<IActionResult> Tekliler(int page=1)
        {
            var result = await _teklilerServices.GetAll();
            List<ShippingProduct> temp = new List<ShippingProduct>();
            foreach (var item in result.Data)
            {
                ShippingProduct tp = new ShippingProduct();
                var p1 = await _productServices.GetById(item.ProductId);
                var c1 = await _colorServices.GetById(item.ColorId);
                tp.ProductName = p1.Data.ModelName;
                tp.ColorName = c1.Data.ColorName;
                tp.Count = item.Count;
                tp.TekliId = item.Id;
                tp.ProductId = item.ProductId;
                tp.ModelCode = p1.Data.ModelCode;
                tp.Barcode = p1.Data.Barcode;
                temp.Add(tp);
            }
            ViewBag.ProductCount = result.Data.Count;
            return View(temp);
            //var result = await _teklilerServices.TekliListesi();
            //return View(result.ToPagedList(page,12));
        }
        public async Task<IActionResult> Deneme()
        {
          
            return View();
        }
        public async Task<IActionResult> DenemeJson()
        {
            var result = await _teklilerServices.GetAll();
            List<ShippingProduct> temp = new List<ShippingProduct>();
            foreach (var item in result.Data)
            {
                var resq = await _productServices.GetTekliAll(item.Id);
                ShippingProduct tp = new ShippingProduct();
                // var p1 = await _productServices.GetById(item.ProductId);
                //var q = await _productRepository.GetAllAsync();
                //var c1 = await _colorServices.GetById(item.ColorId);
                if (resq.ProductName!=null)
                {
                    tp.ProductName = resq.ProductName;
                    tp.ColorName = resq.ColorName;
                    tp.Count = resq.Count;
                    tp.TekliId = item.Id;
                    tp.ProductId = resq.ProductId;
                    tp.ModelCode = resq.ModelCode;
                    tp.Barcode = resq.Barcode;
                    tp.Age = resq.Age;
                    tp.Gender = (Gender)(int)resq.Gender;
                    temp.Add(tp);
                }
               
            }
            var models = JsonConvert.SerializeObject(temp);
            return Json(models);
        }
        public async Task<IActionResult> TekliEkle(int? page=1, int? colorPage=1)
        {

            PagedListShippingProduct dto = new PagedListShippingProduct();
            var produtList = await _productServices.TekliList();
           var dtos = produtList.ToPagedList(page.Value, 12);
          
            ViewBag.Count= produtList.Count;
            //   var colorList = await _colorServices.GetAll();
            //   dto.ProductColorr = colorList.Data;
            //var dto = colorList.Data.ToPagedList(colorPage.Value,5);
            return View("TekliEkle", dtos);
        } 
        public async Task<IActionResult> TekliColorPartical(int? page=1)
        {
            PagedListShippingProduct dto = new PagedListShippingProduct();
            var colorList = await _colorServices.GetAll();
            //(IPagedList)dto.ProductColor = colorList.Data.ToPagedList(page.Value,5);
            var q = colorList.Data.ToPagedList(page.Value,12);
            ViewBag.ColorCount= colorList.Data.Count;
            dto.pageCount = colorList.Data.Count;
            return PartialView("TekliColorPartical", q);
        }
        public async Task<IActionResult> Tekli(long productId,String body)
        {
            var model = JsonConvert.DeserializeObject<List<ColorDto>>(body);
            if (productId>0)
            {
                foreach (var item in model)
                {

                    item.ColorName = item.ColorName.Replace(",", "");
                        item.Count = item.Count.Replace(",", "");
                        if (item.ColorName != "" && item.Count != "" && item.ColorName != "," && item.Count != "," && item.Count != "on" && item.ColorName != "on")
                        {
                            var result = await _teklilerServices.Add(new Tekliler()
                            {
                                ProductId=productId,
                                ColorId=Convert.ToInt64(item.ColorName),
                                Count= Convert.ToInt64(item.Count),
                                IsActive=true,
                                RegisterDate=DateTime.Now
                            });
                                await _stockServices.Add(new()
                                {
                                    ProductId = productId,
                                    ColorId = Convert.ToInt64(item.ColorName),
                                    StockCount = Convert.ToInt64(item.Count),
                                    RegisterDate = DateTime.UtcNow.ToString(),
                                    StockStatus = StockStatus.UrunEklendi,
                                    TekliMi = true,
                                    tekliId= result,
                                    //TenantId
                                });
                        }
                }
             }
            //var query = await _teklilerServices.GetAll();
            return RedirectToAction("Deneme");

        }

        public async Task<IActionResult> DeleteTekli(long tekliId)
        {
            if (tekliId!=0)
            {
                var tekli = await _teklilerServices.GetById(tekliId);
                await _teklilerServices.Delete(tekli.Data);
               var tekliStok = await _stockServices.GetAll(a=>a.tekliId==tekliId);
                foreach (var item in tekliStok.Data)
                {
                await _stockServices.Delete(item);
                }
            }
            return RedirectToAction("Deneme");
        }
        public async Task<IActionResult> TekliDuzenle(long tekliId)
        {
            if (tekliId != 0)
            {
                var tekli = await _productServices.GetTekliDetay(tekliId);
                return View(tekli);
            }
            return RedirectToAction("Tekliler");
        }
        [HttpPost]
        public async Task<IActionResult> TekliDuzenle(ShippingProduct dto)
        {
            if (dto.Count != 0)
            {
                var tekli = await _teklilerServices.GetById(dto.TekliId);
                tekli.Data.Count = dto.Count;
                await _teklilerServices.UpdateAll(tekli.Data);

                //stock güncellemesi
                var stock =await _stockServices.GetAll(a=>a.tekliId==dto.TekliId);
                var result = stock.Data.FirstOrDefault();
                result.StockCount= dto.Count;
                await _stockServices.UpdateAll(result);
            }
            return RedirectToAction("Tekliler");
        }




        public async Task<IActionResult> AgeIndex()
        {
            var result = await _ageRepository.GetAllAsync();
            return View(result);
        }
        public async Task<IActionResult> AgeAdd()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AgeAdd(ProductAges productAges)
        {
            if (productAges.Name!=null)
            {
                await  _ageRepository.CreateAsync(productAges);
                return RedirectToAction("AgeIndex");
            }
            return View();
        }
        public async Task<IActionResult> AgeUpdate(long id)
        {
            var result = await _ageRepository.FindAsync(id);
            return View(result);
        }
        [HttpPost]
        public async Task<IActionResult> AgeUpdate(ProductAges productAges)
        {
            _ageRepository.UpdateAll(productAges);
            return RedirectToAction("AgeIndex");
        }
        public async Task<IActionResult> DeleteAge(long id)
        {
           var reulst = await _ageRepository.FindAsync(id);
            _ageRepository.Remove(reulst);
            return RedirectToAction("AgeIndex");
        }
        //public async Task<IActionResult> TekliEkle(String body)
        //{
        //    return View(dto);
        //}
        //public async Task<IActionResult> AddColor(Color color)
        //{
        //    await _colorServices.Add(color);
        //    return View("Color");
        //}
    }
}
