using Application.Interface;
using AutoMapper;
using Core.Entities;
using DataAccess.Interfaces;
using Entities.Concrete;
using Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Models;
using static Entities.Enums.Enums;

namespace WebApp.Controllers
{
    [Authorize(Policy = "AdminMagazaDepo")]
    public class HomeController : Controller
    {

        readonly IProductServices _productServices;
        readonly IStockServices _stockServices;
        readonly IColorServices _colorServices;
        readonly IAppUserServices _appUserServices;
        readonly IOrderServices _orderServices;
        readonly IMapper _mapper;
        readonly IProductAgeDal _productAge;

        public HomeController(IProductServices productServices, IStockServices stockServices, IMapper mapper, IColorServices colorServices, IAppUserServices appUserServices, IOrderServices orderServices, IProductAgeDal productAge)
        {
            _mapper = mapper;
            _productServices = productServices;
            _stockServices = stockServices;
            _colorServices = colorServices;
            _appUserServices = appUserServices;
            _orderServices = orderServices;
            _productAge = productAge;
        }

        public async Task< IActionResult> UrunEkle(bool ? returnView=null)
        {
            ViewBag.StockAdd = true;
            ViewBag.Gender = GetEnumList();
            ViewBag.Age = GetEnumList2();
        ProductColorDto dto = new ProductColorDto();
            var gelen = await _productServices.GetStockAndProduct();
            dto.Products= (await _productServices.GetAll()).Data;
            dto.ProductColor =((await _colorServices.GetAll()).Data);
          ViewBag.selectList = new SelectList(((await _colorServices.GetAll()).Data), "Id", "ColorName");
          ViewBag.productAge = new SelectList(((await _productAge.GetAllAsync())), "Id", "Name");
            return View(dto);
        }
        SelectList GetEnumList()
        {
            var list = from Gender e in Enum.GetValues(typeof(Gender))
                       select new { EnumId = (int)e, Gender = e.ToString() };
            return new SelectList(list, "EnumId", "Gender", 1);

        }
        SelectList GetEnumList2()
        {
            var list = from Age e in Enum.GetValues(typeof(Age))
                       select new { EnumId = (int)e, Age = e.ToString() };
            return new SelectList(list, "EnumId", "Age", 1);

        }
        public async Task<IActionResult> GetProductListJson(string a="")
        {
            var result = await _productServices.GetAll();
            var qq = JsonConvert.SerializeObject(result.Data);
            return Json(qq);
        }

        public async Task<IActionResult> Index1(bool? returnView = null)
        {
           
            IndexDto dto = new IndexDto();
            var result = await _appUserServices.GetUsersAndRoles();
            dto.UserAndRoleDto = result;

            return View(dto);
        }

        public async Task<IActionResult> DateReturn()
        {
            var date = await _orderServices.SP_GenelKazanc();
            decimal toplam = 0;
            foreach (var item in date)
            {
                toplam += item.Miktar;
            }
            for (int i = 0; i < date.Count; i++)
            {
                string result = date[i].Miktar.ToString().Replace('.',',');
                var split = result.Split(',');
                date[i].Miktar = decimal.Parse(split[0].ToString(), CultureInfo.CurrentCulture);

            }
            ViewBag.Toplam = toplam;
            return Json(date);
        }

        public async Task <IActionResult> Uyan()
        {
            return Json("");
        }
        //public  async Task<IActionResult> ProductAdd(CreateProductDto createProductDto)
        //{

        //   var productId=  await _productServices.Add(_mapper.Map<Product>(createProductDto));
        //    //if (productId != 0)
        //    //{
        //    //    Stock stock = new Stock { TenantId = 1,RegisterDate=DateTime.Now.ToString(),ProductId= productId ,StockCount=Convert.ToInt64(createProductDto.ModelCount)};
        //    //    var result = await _productServices.AddStockCount(stock);
        //    //    //var result = await _productServices.AddStockCount(productId, );
        //    //}
        //    ProductColorDto dto = new ProductColorDto();
        //    var obje = await _productServices.GetById(productId);
        //    dto.SingleProduct=obje.Data;
        //    dto.ProductColor = ((await _colorServices.GetAll()).Data);
        //    ViewBag.ProductId = productId;
        //    ViewBag.ColorCount = (await _colorServices.GetAll()).Data.Count;

        //    return View("AddColorForProduct", dto); 
        //}

        //public async Task<IActionResult> AddColorForProduct(long productId, String body, bool? update=null)
        //{
        //    var model = JsonConvert.DeserializeObject<List<ColorDto>>(body);
        //    foreach (var item in model)
        //    {

        //        item.ColorName = item.ColorName.Replace(",", "");
        //        item.Count = item.Count.Replace(",", "");
        //        if (update!=null&&update==true)
        //        {
        //            if (item.ColorName != "" && item.Count!=""&&item.ColorName != "," && item.Count != ",")
        //            {

        //                var stockIds =(await  _stockServices.GetAll(a => a.ProductId == productId && a.ColorId ==Convert.ToInt64(item.ColorName))).Data.FirstOrDefault(a=>a.ColorId== Convert.ToInt64(item.ColorName)&&a.ProductId==productId);
        //                await _stockServices.UpdateAll(new()
        //                {
        //                    Id= stockIds.Id,
        //                    ProductId = productId,
        //                    ColorId = Convert.ToInt64(item.ColorName),
        //                    StockCount = Convert.ToInt64(item.Count),
        //                    RegisterDate = DateTime.UtcNow.ToString(),
        //                    StockStatus = StockStatus.UrunEklendi,
        //                    //TenantId
        //                });
        //            }

        //        }
        //        else
        //        {
        //            if (item.ColorName != "" && item.Count != "" && item.ColorName != "," && item.Count != ",")
        //            {
        //                await _stockServices.Add(new()
        //                {
        //                    ProductId = productId,
        //                    ColorId = Convert.ToInt64(item.ColorName),
        //                    StockCount = Convert.ToInt64(item.Count),
        //                    RegisterDate = DateTime.UtcNow.ToString(),
        //                    StockStatus = StockStatus.UrunEklendi,
        //                    //TenantId
        //                });
        //            }
        //        }

        //    }
        //    return RedirectToAction("GetAllProductAndStock");
        //}
        //public async Task<IActionResult> GetAllProductAndStock()
        //{
        //    var result = await _productServices.GetStockAndProduct();
        //    return View(result);
        //}
        //public async Task<IActionResult> ProductStockAdd(ProductStockAdd productStockAdd)
        //{
        //  //var result = await _productServices.AddStockCount(productStockAdd.ProductId,productStockAdd.StockCount);
        //  //  if (result)
        //  //  {
        //  //      ViewBag.StockAdd = true;
        //  //      return View("Index", (await _productServices.GetStockAndProduct()));
        //  //  }
        //    return View("Index", (await _productServices.GetAll()).Data);
        //}
        //public async Task<IActionResult> UpdateProductStock(long productId)
        //{
        //    var result = await _productServices.GetProductStockUpdate(productId);
        //    ViewBag.ProductId = productId;
        //    ViewBag.ColorCount = result.Count;
        //    return View(result);
        //}
        //public async Task<IActionResult> AddColor(Color color)
        //{
        //    await _colorServices.Add(color);
        //    return View("Color");
        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public async Task<IActionResult> Color()
        {
            return View();
        } 
       
    }
}
