using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;
using System.IO;
using System.Threading;
using System.Data.SqlClient;

namespace HtmlGeneratorServices.ServicesHelper
{
    public class MailSender
    {
        static string _SMTPServer = "smtp.office365.com";
        static string _SMTPUser = "no-reply@bcalife.co.id";
        static string _SMTPPassword = "Jakarta1!";
        static int _SMTPPort = 25;
        static string _MailSender = "no-reply@bcalife.co.id";
        static string ftpRemoteFolder = System.Configuration.ConfigurationManager.AppSettings["ftpRemoteFolder"];
        static string CC_Address = System.Configuration.ConfigurationManager.AppSettings["ccEmailReceip"];
        static string tempFileLocation = @"D:\TemporaryFiles\";

        static int maxTries = 3;
        public static void SendMail(string destination, string spajNumber, bool isHttpPost)
        {
            var t = new Thread(() => StartEmailWorker(destination, spajNumber, isHttpPost));
            t.Start();
        }

        public static void SendResubmitMail(string destination, string spajNumber, bool isHttpPost)
        {
            var t = new Thread(() => StartResubmitEmailWorker(destination, spajNumber, isHttpPost));
            t.Start();
        }

        static void StartResubmitEmailWorker(string destination, string spajNumber, bool isHttpPost)
        {
            if (isHttpPost)
            {
                try
                {
                    string[] files = Directory.GetFiles(@"C:\inetpub\wwwroot\SPAJFiles\" + spajNumber + "\resubmit");

                    LogWriter.WriteLog("Config Email Logic for " + spajNumber);
                    SmtpClient smtpClient = new SmtpClient();
                    MailMessage message = new MailMessage(_MailSender, destination);
                    message.Subject = "Resubmit Files for SPAJ Number " + spajNumber;
                    message.Body = "This is the resubmitted file for SPAJ Number " + spajNumber + ". Please do not reply to this email. Any concern related with the file, please contact your system Administrator";
                    smtpClient.Host = _SMTPServer;
                    smtpClient.EnableSsl = true;
                    NetworkCredential credential = new NetworkCredential(_SMTPUser, _SMTPPassword);
                    smtpClient.Credentials = credential;
                    smtpClient.Port = _SMTPPort;
                    smtpClient.Timeout = 60000;
                    LogWriter.WriteLog("Done Configuring email for " + spajNumber);
                    if (files.Length > -1)
                    {
                        LogWriter.WriteLog("Adding Attachment for " + spajNumber);
                        for (int i = 0; i < files.Length; i++)
                        {
                            Attachment attachment = new Attachment(files[i], MediaTypeNames.Application.Octet);
                            ContentDisposition disposition = attachment.ContentDisposition;
                            disposition.CreationDate = File.GetCreationTime(files[i]);
                            disposition.ModificationDate = File.GetLastWriteTime(files[i]);
                            disposition.ReadDate = File.GetLastAccessTime(files[i]);
                            disposition.FileName = Path.GetFileName(files[i]);
                            disposition.Size = new FileInfo(files[i]).Length;
                            disposition.DispositionType = DispositionTypeNames.Attachment;
                            message.Attachments.Add(attachment);
                        }
                        LogWriter.WriteLog("Done Adding Attachment for " + spajNumber);
                        LogWriter.WriteLog("Pre Sending Email for " + spajNumber);
                        message.CC.Add(CC_Address);
                        message.CC.Add("js.regar@outlook.com");
                        message.CC.Add("ariani.pondaag@infoconnect.com.my");
                        message.CC.Add("infoipadair203@gmail.com");
                        message.CC.Add("julius_luthena @bcalife.co.id");
                        smtpClient.Send(message);
                        LogWriter.WriteLog("Done Sending Email for " + spajNumber);
                        smtpClient.Dispose();
                        message.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        static void StartEmailWorker(string destination, string spajNumber, bool isHttpPost)
        {
            //Thread.Sleep(180000);
            int count = 0;
            //while (true)
            //{
            //if (count > 0)
            //{
            //    //put a delay on the thread if this is a retry.
            //    Thread.Sleep(60000);
            //}

            if (!isHttpPost)
            {
                try
                {
                    LogWriter.WriteLog("Get File List from remote FTP for attachment for " + spajNumber);
                    string[] remoteFiles = FTPHelper.GetFileList(@ftpRemoteFolder + "/" + spajNumber);
                    //string[] remoteFiles = Directory.GetFiles(@"C:\inetpub\wwwroot\SPAJFiles" + spajNumber);
                    List<string> attachments = new List<string>();
                    for (int i = 0; i < remoteFiles.Length; i++)
                    {
                        string localFile = tempFileLocation + remoteFiles[i]; //AppDomain.CurrentDomain.BaseDirectory + "\\" + remoteFiles[i];
                        FTPHelper.DownloadFTPFile(@ftpRemoteFolder + "/" + spajNumber + "/" + remoteFiles[i], localFile);
                        attachments.Add(localFile);
                    }
                    //sending mail logic
                    LogWriter.WriteLog("Config Email Logicfor " + spajNumber);
                    SmtpClient smtpClient = new SmtpClient();
                    MailMessage message = new MailMessage(_MailSender, destination);
                    message.Subject = "Files for SPAJ Number " + spajNumber;
                    message.Body = "This is the file related with SPAJ Number " + spajNumber + ". Please do not reply to this email. Any concern related with the file, please contact your system Administrator";
                    smtpClient.Host = _SMTPServer;
                    smtpClient.EnableSsl = true;
                    NetworkCredential credential = new NetworkCredential(_SMTPUser, _SMTPPassword);
                    smtpClient.Credentials = credential;
                    smtpClient.Port = _SMTPPort;
                    smtpClient.Timeout = 60000;
                    LogWriter.WriteLog("Done Configuring email for " + spajNumber);
                    if (attachments.Count > -1)
                    {
                        LogWriter.WriteLog("Adding Attachment for " + spajNumber);
                        for (int i = 0; i < attachments.Count; i++)
                        {
                            Attachment attachment = new Attachment(attachments[i], MediaTypeNames.Application.Octet);
                            ContentDisposition disposition = attachment.ContentDisposition;
                            disposition.CreationDate = File.GetCreationTime(attachments[i]);
                            disposition.ModificationDate = File.GetLastWriteTime(attachments[i]);
                            disposition.ReadDate = File.GetLastAccessTime(attachments[i]);
                            disposition.FileName = Path.GetFileName(attachments[i]);
                            disposition.Size = new FileInfo(attachments[i]).Length;
                            disposition.DispositionType = DispositionTypeNames.Attachment;
                            message.Attachments.Add(attachment);
                        }
                        LogWriter.WriteLog("Done Adding Attachment for " + spajNumber);
                        LogWriter.WriteLog("Pre Sending Email for " + spajNumber);
                        message.CC.Add(CC_Address);
                        message.CC.Add("js.regar@outlook.com");
                        message.CC.Add("ariani.pondaag@infoconnect.com.my");
                        message.CC.Add("infoipadair203@gmail.com");
                        message.CC.Add("julius_luthena @bcalife.co.id");
                        smtpClient.Send(message);
                        LogWriter.WriteLog("Done Sending Email for " + spajNumber);
                        smtpClient.Dispose();
                        message.Dispose();

                        //delete file after sending
                        for (int i = 0; i < attachments.Count; i++)
                        {
                            File.Delete(attachments[i]);
                        }
                    }
                    //break;
                }
                catch (Exception ex)
                {
                    if (++count > maxTries)
                    {
                        //write into DB for this log
                        LogWriter.WriteLog("Fail Sending email for SPAJ Number " + spajNumber);

                        string updateStatement = "UPDATE TBM_SPAJ_NUMBER SET EmailStatus = 'Failed to deliver' WHERE SPAJCode =" + spajNumber;
                        SqlConnection conn = E_Submission.ESubmissionHelper.Database.GetSqlConnection();
                        conn.Open();
                        SqlCommand command = new SqlCommand(updateStatement, conn);
                        command.ExecuteNonQuery();
                        conn.Close();
                        //break;
                    }
                }
            }
            else
            {
                try
                {
                    string[] files = Directory.GetFiles(@"C:\inetpub\wwwroot\SPAJFiles\" + spajNumber);

                    LogWriter.WriteLog("Config Email Logic for " + spajNumber);
                    SmtpClient smtpClient = new SmtpClient();
                    MailMessage message = new MailMessage(_MailSender, destination);
                    message.Subject = "Files for SPAJ Number " + spajNumber;
                    message.Body = "This is the file related with SPAJ Number " + spajNumber + ". Please do not reply to this email. Any concern related with the file, please contact your system Administrator";
                    smtpClient.Host = _SMTPServer;
                    smtpClient.EnableSsl = true;
                    NetworkCredential credential = new NetworkCredential(_SMTPUser, _SMTPPassword);
                    smtpClient.Credentials = credential;
                    smtpClient.Port = _SMTPPort;
                    smtpClient.Timeout = 60000;
                    LogWriter.WriteLog("Done Configuring email for " + spajNumber);
                    if (files.Length > -1)
                    {
                        LogWriter.WriteLog("Adding Attachment for " + spajNumber);
                        for (int i = 0; i < files.Length; i++)
                        {
                            Attachment attachment = new Attachment(files[i], MediaTypeNames.Application.Octet);
                            ContentDisposition disposition = attachment.ContentDisposition;
                            disposition.CreationDate = File.GetCreationTime(files[i]);
                            disposition.ModificationDate = File.GetLastWriteTime(files[i]);
                            disposition.ReadDate = File.GetLastAccessTime(files[i]);
                            disposition.FileName = Path.GetFileName(files[i]);
                            disposition.Size = new FileInfo(files[i]).Length;
                            disposition.DispositionType = DispositionTypeNames.Attachment;
                            message.Attachments.Add(attachment);
                        }
                        LogWriter.WriteLog("Done Adding Attachment for " + spajNumber);
                        LogWriter.WriteLog("Pre Sending Email for " + spajNumber);
                        message.CC.Add(CC_Address);
                        message.CC.Add("js.regar@outlook.com");
                        message.CC.Add("ariani.pondaag@infoconnect.com.my");
                        message.CC.Add("infoipadair203@gmail.com");
                        message.CC.Add("julius_luthena @bcalife.co.id");
                        smtpClient.Send(message);
                        LogWriter.WriteLog("Done Sending Email for " + spajNumber);
                        smtpClient.Dispose();
                        message.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " " + spajNumber);
                }
            }
        }
    }
}