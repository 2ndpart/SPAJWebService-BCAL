using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Net;
using System.IO;

namespace HtmlGeneratorServices.ServicesHelper
{
    public class FTPHelper
    {
        //get FTP host from config file

        static string ftpPath = System.Configuration.ConfigurationManager.AppSettings["ftpServer"];
        static string ftpUser = System.Configuration.ConfigurationManager.AppSettings["ftpUser"];
        static string ftpPass = System.Configuration.ConfigurationManager.AppSettings["ftpPass"];
        static string ftpRemoteFolder = System.Configuration.ConfigurationManager.AppSettings["ftpRemoteFolder"];
        static string ftpBackupFolder = System.Configuration.ConfigurationManager.AppSettings["ftpBackupFolder"];
        public static string DownloadFTPFile(string remoteFTPFile, string localFTPFile)
        {
            string fullFtpPath = ftpPath + "/" + remoteFTPFile;
            try
            {
                var reqFtp = (FtpWebRequest)WebRequest.Create(new Uri(fullFtpPath));
                reqFtp.UseBinary = true;
                reqFtp.Credentials = new NetworkCredential(ftpUser, ftpPass);
                reqFtp.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFtp.Proxy = null;
                reqFtp.KeepAlive = false;
                reqFtp.UsePassive = true;
                LogWriter.WriteLog("Pre Download " + remoteFTPFile);
                Stream reader = reqFtp.GetResponse().GetResponseStream();
                FileStream fileStream = new FileStream(localFTPFile, FileMode.Create);
                int bytesRead = 0;
                byte[] buffer = new byte[2048];
                while (true)
                {
                    bytesRead = reader.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                        break;

                    fileStream.Write(buffer, 0, bytesRead);
                }
                fileStream.Close();
                LogWriter.WriteLog("Done downloading " + remoteFTPFile);
                return localFTPFile;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            
        }

        public static string[] GetFileList(string path)
        {
            //var ftpPath = "ftp://waws-prod-sg1-015.ftp.azurewebsites.windows.net" + "/" + "Staging";
            //var ftpUser = @"MPOSWS\MPOSteam";
            //var ftpPass = "ultima51";

            string fullFtpPath = ftpPath + "/" + path; ;
            
            var result = new StringBuilder();
            try
            {
                var strLink = fullFtpPath;
                var reqFtp = (FtpWebRequest)WebRequest.Create(new Uri(strLink));
                reqFtp.UseBinary = true;
                reqFtp.Credentials = new NetworkCredential(ftpUser, ftpPass);
                reqFtp.Method = WebRequestMethods.Ftp.ListDirectory;
                reqFtp.Proxy = null;
                reqFtp.KeepAlive = false;
                reqFtp.UsePassive = true;
                using (var response = reqFtp.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var line = reader.ReadLine();
                        while (line != null)
                        {
                            result.Append(line);
                            result.Append("\n");
                            line = reader.ReadLine();
                        }
                        result.Remove(result.ToString().LastIndexOf('\n'), 1);
                    }
                }
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                Console.WriteLine("FTP ERROR: ", ex.Message);
                return null;
            }
        }

        public static string CreateBackupFolder(string folderName)
        {
            try
            {
                WebRequest request = WebRequest.Create(ftpPath + "/" + ftpBackupFolder + "/" + folderName);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = new NetworkCredential(@ftpUser, ftpPass);
                using (var resp = (FtpWebResponse)request.GetResponse())
                {
                    Console.WriteLine(resp.StatusCode);
                }
                return folderName;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string CreateFolder(string spajNumber)
        {
            try
            {
                WebRequest request = WebRequest.Create(ftpPath + "/" + ftpRemoteFolder + "/" + spajNumber);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = new NetworkCredential(@ftpUser, ftpPass);
                using (var resp = (FtpWebResponse)request.GetResponse())
                {
                    Console.WriteLine(resp.StatusCode);
                }
                return spajNumber;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}