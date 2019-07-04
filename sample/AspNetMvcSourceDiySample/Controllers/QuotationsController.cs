using AspNetMvcSourceDiySample.Extensions.ModelBinders.DynGrid;
using AspNetMvcSourceDiySample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AspNetMvcSourceDiySample.Controllers
{
    public class QuotationsController : Controller
    {
        // GET: Quotations
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddQuotations()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddQuotations([ModelBinder(typeof(DynamicGridModelBinder))] AddQuotaViewModel viewModel, FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("NoResponse", new { page = string.Empty });
            }
            //return null;
            return View();
        }
    }
}