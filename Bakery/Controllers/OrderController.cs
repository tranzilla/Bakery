using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bakery.Models;


namespace Bakery.Controllers
{
    public class OrderController : Controller
    {
        BakeryEntities db = new BakeryEntities();
        // GET: Order
        public ActionResult Index()
        {
            if (Session["PersonKey"] == null)
            {
                Message m = new Message();
                m.MessageText = "You must Login to place an order";
                return RedirectToAction("Result", m); //this will redirect the user to the login page if they did not log in
            }

            Order o = new Order();
            // this will allow the order object to be retain in the sessions and not lose all the values when it gets refreshes 
            Session["orders"] = o;
            //viewbag to send product list for dropdown in form
            ViewBag.products = new SelectList(db.Products, "ProductKey", "ProductName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "ProductKey, ProductName, Price, Quantity, Discount")]Item i)
        {
            var prod = from p in db.Products
                       where p.ProductKey == i.ProductKey
                       select new { p.ProductName, p.ProductPrice };

            foreach (var pr in prod)
            {
                i.ProductName = pr.ProductName.ToString();
                i.Price = (decimal)pr.ProductPrice;
            }

            Order o = (Order)Session["Orders"];
            o.AddItem(i);
            Session["Orders"] = o;
            ViewBag.products = new SelectList(db.Products, "ProductKey", "ProductName");
            return View();

        }
        public ActionResult FinishOrder()
        {
            //This will write the sale and saledetail to the database
            //and then pass the order onto the Reciept view
            Sale sale = new Sale();
            sale.EmployeeKey = 1;
            sale.SaleDate = DateTime.Now;
            sale.CustomerKey = (int)Session["PersonKey"];
            db.Sales.Add(sale);

            //get the order back from the Session
            Order o = (Order)Session["orders"];
            //get the items list from the order object
            List<Item> saleItems = o.GetItems();
            //Loop through them and write the values
            //to the SaleDetails object
            foreach (Item i in saleItems)
            {
                SaleDetail sd = new SaleDetail();
                sd.Sale = sale;
                sd.ProductKey = i.ProductKey;
                sd.SaleDetailPriceCharged = i.Price;
                sd.SaleDetailQuantity = i.Quantity;
                sd.SaleDetailDiscount = (decimal)i.Discount;
                sd.SaleDetailSaleTaxPercent = .09m;
                sd.SaleDetailEatInTax = .01m;

                db.SaleDetails.Add(sd);
            }
            //save all the changes
            db.SaveChanges();

            //Make sure all the calculations are done
            //before passing the Orders object
            //to the reciept
            o.CalculateSubTotal();
            o.CalculateDiscount();
            o.CalculateSubAfterDiscount();
            o.CalculateTax();
            o.CalculateTotal();

            return View("Receipt", o);
        }

        public ActionResult Receipt(Order order)
        {
            return View(order);
        }

        public ActionResult Result(Message m)
        {
            return View(m);
        }
    }
}