using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace AndApp.Utilities
{
    public class LogU
    {
        public static void WriteLog(string message)
        {
            try
            {
                StringBuilder sbText = new StringBuilder();
                sbText.AppendLine("===============================================================================");
                sbText.AppendLine("Date    : " + DateTime.UtcNow.ToString());
                sbText.AppendLine("Message : " + message);
                sbText.AppendLine();
                LogToFile(string.Empty, sbText.ToString());
            }
            catch  (Exception ex)
            {
                LogToFile("LogU >> WriteLog", ex.ToString());
            }
        }

        public static void WriteLog(Exception ex)
        {
            try
            {
                StackTrace track = new StackTrace(ex, true);

                StringBuilder sbText = new StringBuilder();
                sbText.AppendLine("=====================================================================================");
                sbText.AppendLine("Date : " + DateTime.UtcNow.ToString());

                if (track != null && track.FrameCount != 0)
                {
                    try
                    {
                        sbText.AppendLine("Method : " + track.GetFrame(0).GetMethod().Name);
                        sbText.AppendLine("Line : " + track.GetFrame(0).GetFileLineNumber().ToString());
                        sbText.AppendLine("Column : " + track.GetFrame(0).GetFileColumnNumber().ToString());
                    }
                    catch
                    {

                    }
                }
                sbText.AppendLine("Error : " + ex.Message);
                sbText.AppendLine("Stack : " + ex.StackTrace);
                sbText.AppendLine();

                LogToFile(string.Empty, sbText.ToString());
            }
            catch
            {

            }
        }

        private static void LogToFile(string fileName, string message)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = "Log.txt";
            }
            //string logPath = "D:\\Projects\\And\\AndApp\\AndApp\\Log\\";
            string logPath = ConfigurationManager.AppSettings["LogStorage"].ToString();
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            string file = logPath + "//" + fileName;
            bool flag = false;

            FileInfo info = new FileInfo(file);
            if (info.Exists)
            {
                double size = info.Length / 1048576;
                if (size > 1.5)
                {
                    string destfile = file.Substring(0, file.Length - 4) + "_" + DateTime.UtcNow.ToString("dd-MM-yyyy") + ".txt";
                    File.Move(file, destfile);
                    flag = false;
                }
                else
                {
                    flag = true;
                }
            }

            if (!flag)
            {
                using (StreamWriter sw = File.CreateText(file))
                    sw.WriteLine(message);
            }
            else
            {
                using (StreamWriter sw = File.AppendText(file))
                    sw.WriteLine(message);
            }
        }
    }
}