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
            return AddQuotationsPostCore(viewModel);
        }

        public ActionResult AddQuotationsTranditional()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddQuotationsTranditional(AddQuotaViewModel viewModel, FormCollection form)
        {
            return AddQuotationsPostCore(viewModel);
        }

        private ActionResult AddQuotationsPostCore(AddQuotaViewModel viewModel)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                viewModel = viewModel,
                modelState_IsValid = this.ModelState.IsValid,
                modelState_Values = this.ModelState.Values,
            }, Newtonsoft.Json.Formatting.Indented);
            return Content(json);
        }
    }
}