using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HtmlGeneratorServices
{
    public class ProductInformationModel
    {
        string filePath;
        string dateModified;
        string fileSize;
        string fileType;

        public string FilePath
        {
            get { return this.filePath; }
            set { this.filePath = value; }
        }

        public string DateModified
        {
            get { return this.dateModified; }
            set { this.dateModified = value; }
        }

        public string FileSize
        {
            get { return this.fileSize; }
            set { this.fileSize = value; }
        }

        public string FileType
        {
            get { return this.fileType; }
            set { this.fileType = value; }
        }
    }
}