using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bakery.Models;

namespace Bakery.Controllers
{
    public class ProductController : Controller
    {
        // //initialized entities - making Bakery database to a class to allow data to be pass through
        BakeryEntities db = new BakeryEntities();
        public ActionResult Product()
        {
            //this will pass the collection product to the index as a list
            return View(db.Products.ToList());
        }
    }
}