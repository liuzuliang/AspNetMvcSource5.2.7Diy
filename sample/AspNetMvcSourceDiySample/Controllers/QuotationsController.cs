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

        public ActionResult List(QuotaQueryCondition queryCondition)
        {
            string json = GetJson(queryCondition);
            return Content(json);
        }

        public ActionResult UserInfo([ModelBinder(typeof(DynamicGridModelBinder))] UserQueryCondition queryCondition)
        {
            string json = GetJson(queryCondition);
            return Content(json);
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
            string json = GetJson(new
            {
                viewModel = viewModel,
                modelState_IsValid = this.ModelState.IsValid,
                modelState_Values = this.ModelState.Values,
            });
            return Content(json);
        }

        private string GetJson(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
        }
    }
}