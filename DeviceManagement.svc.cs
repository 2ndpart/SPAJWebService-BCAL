using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using PetaPoco;
using System.IO;
using E_Submission.Models;
using SPAJ.Models;
using System.Web.Script.Serialization;
using HtmlGeneratorServices.Models;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using HtmlGeneratorServices.ServicesHelper;

namespace HtmlGeneratorServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "DeviceManagement" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select DeviceManagement.svc or DeviceManagement.svc.cs at the Solution Explorer and start debugging.
    public class DeviceManagement : IDeviceManagement
    {
        public void DoWork()
        {
        }

        
    }
}
