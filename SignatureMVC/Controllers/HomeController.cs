using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Drawing;
using System.IO;
using Aspose.Pdf;

namespace SignatureMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ReportView()
        {
            return View();
        }

        public ActionResult ReportLoadView()
        {
            return View();
        }

        [HttpPost]
        public ActionResult FileUpload(List<String> Roles)
        {
            var role = Roles[0];
            var base64 = role.Split(',')[1];
            byte[] bytes = Convert.FromBase64String(base64);
            string strReportName = "Bericht.rpt";
            ReportDocument rd = new ReportDocument();
            string strRptPath = System.Web.HttpContext.Current.Server.MapPath("~/") + "App_Data//" + strReportName;
            rd.Load(strRptPath);            
            Stream stream = rd.ExportToStream(ExportFormatType.PortableDocFormat);// ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, "crReport");
            Document pdfDocument = new Document(stream);
            Aspose.Pdf.Text.TextFragmentAbsorber textFragmentAbsorber = new Aspose.Pdf.Text.TextFragmentAbsorber();
            pdfDocument.Pages.Accept(textFragmentAbsorber);
            Aspose.Pdf.Text.TextFragmentCollection textFragmentCollection = textFragmentAbsorber.TextFragments;
            double llx = 0.0;
            double lly = 0.0;
            double urx = 0.0;
            double ury = 0.0;
            foreach (Aspose.Pdf.Text.TextFragment textFragment in textFragmentCollection)
            {
                if (textFragment.Text.ToLower() == "unterschrift")
                {
                    llx = textFragment.Rectangle.LLX;
                    lly = textFragment.Rectangle.LLY;
                    urx = textFragment.Rectangle.URX;
                    ury = textFragment.Rectangle.URY;
                    textFragment.Text = "";
                }
            }
            Page page = pdfDocument.Pages[1];
            string strImagePath = System.Web.HttpContext.Current.Server.MapPath("~/") + "App_Data//" + "Unterschrift_Set.png";
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                page.Resources.Images.Add(memoryStream);
                page.Contents.Add(new Operator.GSave());
                Aspose.Pdf.Rectangle rectangle = new Aspose.Pdf.Rectangle(llx, lly, llx + 300, lly - 60);
                Matrix matrix = new Matrix(new double[] { rectangle.URX - rectangle.LLX, 0, 0, rectangle.URY - rectangle.LLY, rectangle.LLX, rectangle.LLY });
                page.Contents.Add(new Operator.ConcatenateMatrix(matrix));
                XImage ximage = page.Resources.Images[page.Resources.Images.Count];
                page.Contents.Add(new Operator.Do(ximage.Name));
                page.Contents.Add(new Operator.GRestore());
            }           
            string strOutputPath = System.Web.HttpContext.Current.Server.MapPath("~/") + "App_Data//" + "AddImage_out.pdf";
            pdfDocument.Save(strOutputPath);
            return View();
        }

        [HttpGet]
        public ActionResult ShowReport()
        {
            bool isValid = true;
            string jsonErrorCode = "0";
            string strReportName = "Bericht.rpt";
            string msg = "";
            try
            {
                if (isValid)
                {
                    ReportDocument rd = new ReportDocument();
                    string strRptPath = System.Web.HttpContext.Current.Server.MapPath("~/") + "App_Data//" + strReportName;
                    rd.Load(strRptPath);
                    rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, "crReport");
                }
                else
                {
                    Response.Write("<H2>Report not found</H2>");
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                jsonErrorCode = "-2";
            }

            return Json(new { result = jsonErrorCode, err = msg }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public void LoadReport()
        {
            string strOutputPath = System.Web.HttpContext.Current.Server.MapPath("~/") + "App_Data//" + "AddImage_out.pdf";
            var filestream = System.IO.File.ReadAllBytes(strOutputPath);
            var stream = new MemoryStream(filestream);
            stream.Position = 0;
            stream.WriteTo(Response.OutputStream);
            Response.AddHeader("Content-Disposition", "Attachment;filename=crReport");
            Response.ContentType = "application/pdf";
        }

        public ActionResult RefreshReport()
        {
            return PartialView("_ReportsDisplay");
        }

        public ActionResult RefreshLoadReport()
        {
            return PartialView("_ReportsLoad");
        }
    }    
}