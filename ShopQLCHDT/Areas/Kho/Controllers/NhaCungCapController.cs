using ShopQLCHDT.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ShopQLCHDT.Areas.Kho.Controllers
{
    public class NhaCungCapController : Controller
    {
        private QLY_SHOPDTDD1Entities db = new QLY_SHOPDTDD1Entities();

        // GET: Kho/NhaCungCap
        public ActionResult Index()
        {
            return View(db.NHACUNGCAP.ToList());
        }

        // GET: Kho/NhaCungCap/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NHACUNGCAP ncc)
        {
            if (ModelState.IsValid)
            {
                db.NHACUNGCAP.Add(ncc);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ncc);
        }

        // GET: Kho/NhaCungCap/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            NHACUNGCAP ncc = db.NHACUNGCAP.Find(id);
            if (ncc == null) return HttpNotFound();
            return View(ncc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(NHACUNGCAP ncc)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ncc).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ncc);
        }

        // (Bạn có thể thêm Action Delete/Details tương tự)

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
