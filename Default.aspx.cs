using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace HtmlGeneratorServices
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            String isResubmit = Request.Params["isResubmit"];
            String folderName = Request.Params["folderName"];
            HttpPostedFile file = Request.Files["file"];
            String fileName = folderName + ".zip";
            String baseFileLocation = @"C:\Users\Public\";
            String fixedFileLocation = @"C:\inetpub\wwwroot\SPAJFiles\";
            try
            {
                file.SaveAs(baseFileLocation + fileName);
                File.Move(baseFileLocation + fileName, fixedFileLocation + fileName);
                FastZip fastZip = new FastZip();
                string fileFilter = null;
                if (isResubmit.Equals("true"))
                {
                    fastZip.ExtractZip(fixedFileLocation + fileName, fixedFileLocation + folderName + "\\resubmit", fileFilter);
                }
                else
                {
                    fastZip.ExtractZip(fixedFileLocation + fileName, fixedFileLocation + folderName, fileFilter);
                }
                File.Delete(fixedFileLocation + fileName);
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
        }
    }
}