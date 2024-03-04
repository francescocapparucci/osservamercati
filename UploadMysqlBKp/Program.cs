using MySql.Data.MySqlClient;
using System;
using System.Data;
using Renci.SshNet;
using System.IO;
using log4net;
using System.Linq;
using static System.Net.WebRequestMethods;
using System.Net;
using System.Net.FtpClient;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using Osserva.CentraleRischi.Library.Import;

namespace UploadMysqlBKp
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string constring = "server=localhost;user=root;pwd=root;database=addessisrl";
        static void Main(string[] args)
        {

           //uploadClient();
            restoreMysql();
            //deleteFileMysqlMof();
        }

        private static void deleteFileMysqlMof()
        {
            try
            {
                string[] filesindirectory = Directory.GetFiles(@"C:\Users\Administrator\Desktop\FTP\mysqlbackup");
                foreach (string el in filesindirectory)
                {
                    Log.Info("file " + el);
                    Console.WriteLine("file " + el);
                }

                if (filesindirectory != null)
                {
                    foreach (string uploadFile in filesindirectory)
                    {
                        if (uploadFile.Contains("please"))
                        {
                            continue;
                        }
                        int indiceFine = uploadFile.IndexOf(".");
                        int indiceInizio = 47;
                        int lunghezza = indiceFine - indiceInizio;
                        Console.WriteLine("file = " + uploadFile + " inizio = " + indiceInizio + " fine = " + indiceFine + " lunghezza = " + lunghezza);
                        string[] arrLine = System.IO.File.ReadAllLines(uploadFile);
                        arrLine[2 - 1] = " USE `" + uploadFile.Substring(indiceInizio, lunghezza) + "`;";
                        System.IO.File.WriteAllLines(uploadFile, arrLine);
                        Console.WriteLine(" nome file modificato = " + uploadFile.Substring(indiceInizio, lunghezza));
                        try
                        {
                            DBController.ExecMySql("delete from vendite ", "localhost", uploadFile, "root", "tgbyhN19");
                            DBController.ExecMySql("delete from anagrafiche ", "localhost", uploadFile, "root", "tgbyhN19");
                            DBController.ExecMySql("delete from saldi ", "localhost", uploadFile, "root", "tgbyhN19");

                            Log.Info("delete db : " + uploadFile+" Mysql");
                        }
                        catch (MySqlException e)
                        {
                            Log.Error(e);
                            Console.WriteLine(e);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                            Console.WriteLine(ex);
                        }
                    }
                }
                else
                {
                    Log.Info("FTP Vuoto");
                }
            }
            catch (Exception ex)
            {
                Log.Error("errore 1234 : " + ex.ToString());
            }
        }

        private static void restoreMysql()// prende i file .sql da directory e restora su mysql
        {
            Log.Info("START restoreMysql ");
            try  
            {
//                string[] filesindirectory = Directory.GetFiles(@"C:\Users\Administrator\Desktop\FTP\mysqlbackup");
                string[] filesindirectory = Directory.GetFiles(@"C:\Users\franc\Desktop\bkp");

                foreach (string el in filesindirectory)
                {
                    Log.Info("file " + el);
                    Console.WriteLine("file " + el);
                }

                if (filesindirectory != null)
                {
                    foreach (string uploadFile in filesindirectory)
                    {
                        if (uploadFile.Contains("please"))
                        {
                            continue;
                        }
                        int indiceFine = uploadFile.IndexOf(".");
                        int indiceInizio = 47;
                        int lunghezza = indiceFine - indiceInizio;
                        Console.WriteLine("file = " + uploadFile + " inizio = " + indiceInizio + " fine = " + indiceFine + " lunghezza = " + lunghezza);
                        string[] arrLine = System.IO.File.ReadAllLines(uploadFile);
                        arrLine[2 - 1] = " USE `" + uploadFile.Substring(indiceInizio, lunghezza) + "`;";
                        System.IO.File.WriteAllLines(uploadFile, arrLine);
                        Console.WriteLine(" nome file modificato = " + uploadFile.Substring(indiceInizio, lunghezza));
                        try
                        {
                            using (MySqlConnection conn = new MySqlConnection(constring))
                            {
                                using (MySqlCommand cmd = new MySqlCommand())
                                {
                                    using (MySqlBackup mb = new MySqlBackup(cmd))
                                    {
                                        cmd.Connection = conn;
                                        conn.Open();
                                        mb.ImportFromFile(uploadFile);
                                        Log.Info("sql caricato con successo");
                                        Console.WriteLine("sql caricato succ " + uploadFile.ToString());
                                        conn.Close();
                                    }
                                }
                            }
                            string returnDelete = DeleteFile(uploadFile.Substring(indiceInizio));
                            Console.WriteLine(returnDelete);
                            Log.Info("delete file : " + returnDelete);
                        }
                        catch (MySqlException e)
                        {
                            Log.Error(e);
                            Console.WriteLine(e);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                            Console.WriteLine(ex);
                        }
                    }
                }
                else
                {
                    Log.Info("FTP Vuoto");
                }
            }catch(Exception ex)
            {
                Log.Error("errore 1234 : " + ex.ToString());
            }
                Log.Info("END restoreMysql ");
        }

        private static string DeleteFile(string fileName)
        {
            Log.Info("START DeleteFile : " + fileName);
            Console.WriteLine("START DeleteFile : " + fileName);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://63.33.245.62"+"//"+ fileName);
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Credentials = new NetworkCredential("support", "support");

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                Log.Info("END DeleteFile : " + fileName);
                Console.WriteLine("END DeleteFile : " + fileName);

                return response.StatusDescription;
            }
        }

        public static void uploadClient()
        {
            Log.Info("start uploadClient");
            try {
                using (var client = new WebClient())
                {
                    string[] filesindirectory = Directory.GetFiles(@"C:\Users\Administrator\Desktop\MYSQL\bkp");
                    Log.Info(Directory.GetFiles(@"C:\Users\Administrator\Desktop\MYSQL\bkp"));

                    foreach (string uploadFile in filesindirectory)
                    {
                        int indice = uploadFile.IndexOf(@"bkp");
                        string filename = uploadFile.Substring(indice + 4);
                        if (filename.Contains("sql"))
                        {
                            client.Credentials = new NetworkCredential("support", "support");
                            Log.Info("client connection");
                            try 
                            { 
                                client.UploadFile("ftp://63.33.245.62/" + filename, WebRequestMethods.Ftp.UploadFile, uploadFile);
                                Log.Debug("success " + filename);
                            }
                            catch (WebException e)
                            
                            { 
                                Console.WriteLine("errore connessione : " + e.ToString()); 
                                Log.Error("errore connessione : " + e.ToString()); 
                            }
                        }
                        else
                        {
                            Console.WriteLine("file non riconosciuto"+filename);
                            Log.Error("error client connection");
                        }
                        Thread.Sleep(100);
                    }
                }
            }catch(Exception e)
            {
                Log.Error(e);
            }
        }
        public static string[] GetFileList()

        {
            string[] downloadFiles;
            StringBuilder result = new StringBuilder();
            FtpWebRequest reqFTP;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + "63.33.245.62" + "/"));
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential("support", "support");
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string line = reader.ReadLine();
                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }
                // to remove the trailing '\n'
                result.Remove(result.ToString().LastIndexOf('\n'), 1);
                reader.Close();
                response.Close();
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                downloadFiles = null;
                return downloadFiles;
            }
        }
        private static void Download(string filePath, string fileName)
        {
            FtpWebRequest reqFTP;
            try
            {
                FileStream outputStream = new FileStream(filePath + "\\" + fileName, FileMode.Create);
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + "63.33.245.62" + "/" + fileName));
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential("support", "support");
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                ftpStream.Close();
                outputStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
