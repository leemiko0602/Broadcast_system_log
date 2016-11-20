//====================================================      
// 文件名称（File Name）：LogWriter.cs
// 功能描述（Description）：将信息异步写入日志
// 数据表（Tables）：无
// 作者（Author）：段志应
// 日期（Create Date）：2012-12-15
//
// 修改记录（Revision History）：
//     R1：
//        修改作者：李利
//        修改日期：2016-10-20
//        修改理由：
//     R2:
//        修改作者：
//        修改日期：
//        修改理由：
//
//====================================================  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Net;
using System.IO;
using System.Net.NetworkInformation;
using System.Configuration;
namespace CUC.CS.BSI.LogLib
{
    /// <summary>
    /// 声明非播放历史日志委托
    /// 一般错误、致命错误、调试、信息、警告
    /// </summary>
    /// <param name="message">消息</param>
    public delegate void AsyncNonHistoryHandler(string message); //delegate-委托
    /// <summary>
    /// 声明播放历史的日志委托
    /// </summary>
    /// <param name="programName">项目名称</param>
    /// <param name="message">消息</param>
    /// <param name="authorName">作者名</param>
    /// <param name="duration">持续时间</param>
    /// <param name="path">路径</param>
    public delegate void AsyncHistoryHandler(string programName, string message, string authorName, string duration, string path);
    public class LogWriter
    {
        //公共变量
        public static string ip =   ConfigurationManager.AppSettings["flumeIP"];
        public static string pingip = ConfigurationManager.AppSettings["flumePingIP"];
        public static string activeDir = ConfigurationManager.AppSettings["logtmpDir"];
        public static string time = DateTime.Now.ToString("yyyy/MM/dd");
        public static string realtime = DateTime.Now.ToString();
        public static string flumestring = "[{\"headers\":{\"remarks\":\"XXXX\"},\"body\":\"{\\\"date_time\\\":\\\"" + time + "\\\"," + "\\\"systemType\\\":\\\"client\\\",";
       
        //log4net日志声明
        public static readonly log4net.ILog nonHistoryLog = log4net.LogManager.GetLogger("nonHistoryLevel");
        public static readonly log4net.ILog historyLog = log4net.LogManager.GetLogger("HistoryLevel");
        #region 委托变量
        /// <summary>
        /// 非播放历史委托
        /// </summary>
        public static AsyncNonHistoryHandler nonHistoryHandler;
        /// <summary>
        /// 播放历史委托
        /// </summary>
        public static AsyncHistoryHandler historyHandler;
        #endregion 委托变量
        /// <summary>
        /// 异步调用，一般错误日志
        /// ERROR
        /// </summary>
        /// <param name="message">消息</param>
        public static void Error(string message)
        {
            //实例委托
            nonHistoryHandler = new AsyncNonHistoryHandler(writeError);
            //异步调用开始，没有回调函数和AsyncState，都为null
            IAsyncResult ia = nonHistoryHandler.BeginInvoke(message, null, null);
        }
        /// <summary>
        /// 异步调用，播放历史日志
        /// HISTORY
        /// </summary>
        /// <param name="programName">项目名称</param>
        /// <param name="message">消息</param>
        /// <param name="authorName">作者</param>
        /// <param name="duration">持续时间</param>
        /// <param name="path">路径</param>
        public static void History(string programName, string message, string authorName, string duration, string path)
        {
            //实例委托
            historyHandler = new AsyncHistoryHandler(writeHistory);
            //异步调用开始，没有回调函数和AsyncState，都为null
            IAsyncResult ia = historyHandler.BeginInvoke(programName, message, authorName, duration, path, null, null);
        }
        /// <summary>
        /// 异步调用，致命错误日志
        /// FATAL
        /// </summary>
        /// <param name="message">消息</param>
        public static void Fatal(string message)
        {
            //实例委托
            nonHistoryHandler = new AsyncNonHistoryHandler(writeFatal);
            //异步调用开始，没有回调函数和AsyncState，都为null
            IAsyncResult ia = nonHistoryHandler.BeginInvoke(message, null, null);
        }
        /// <summary>
        /// 异步调用，调试日志
        /// DEBUG
        /// </summary>
        /// <param name="message">消息</param>
        public static void Debug(string message)
        {
            //实例委托
            nonHistoryHandler = new AsyncNonHistoryHandler(writeDebug);
            //异步调用开始，没有回调函数和AsyncState，都为null
            IAsyncResult ia = nonHistoryHandler.BeginInvoke(message, null, null);
        }
        /// <summary>
        /// 异步调用，一般信息日志
        /// INFO
        /// </summary>
        /// <param name="message">消息</param>
        public static void Info(string message)
        {
            //实例委托
            nonHistoryHandler = new AsyncNonHistoryHandler(writeInfo);
            //异步调用开始，没有回调函数和AsyncState，都为null
            IAsyncResult ia = nonHistoryHandler.BeginInvoke(message, null, null);
        }
        /// <summary>
        /// 异步调用，警告日志
        /// WARN
        /// </summary>
        /// <param name="message">消息</param>
        public static void Warn(string message)
        {
            //实例委托
            nonHistoryHandler = new AsyncNonHistoryHandler(writeWarn);
            //异步调用开始，没有回调函数和AsyncState，都为null
            IAsyncResult ia = nonHistoryHandler.BeginInvoke(message, null, null);
        }

        /// <summary>
        /// 写入日志函数，一般错误类型
        /// </summary>
        /// <param name="message">消息</param>
        private static void writeError(string message)
        {
            //写到本地
            nonHistoryLog.Error(message);
            //写到日志服务器
            writetoFlume("error", message);

        }
        /// <summary>
        /// 写入日志函数，致命错误类型
        /// </summary>
        /// <param name="message">消息</param>
        private static void writeFatal(string message)
        {
            //写到本地
            nonHistoryLog.Fatal(message);
            //写到日志服务器
            writetoFlume("fatal", message);
        }
        /// <summary>
        /// 写入日志函数，播放历史类型
        /// </summary>
        /// <param name="programName">节目名称</param>
        /// <param name="message">消息</param>
        /// <param name="authorName">作者名</param>
        /// <param name="duration">持续时间</param>
        /// <param name="path">路径</param>
        private static void writeHistory(string programName, string message, string authorName, string duration, string path)
        {
            //写到本地
            historyLog.History(programName, message, authorName, duration, path);
            //写到日志服务器
            writeHistoryToFlume(programName, message, authorName, duration, path);


        }
        /// <summary> 
        /// 写入日志函数，调试类型
        /// </summary>
        /// <param name="message">消息</param>
        private static void writeDebug(string message)
        {
            //写到本地
            nonHistoryLog.Debug(message);
            //写到日志服务器
            writetoFlume("debug", message);
        }
        /// <summary>
        /// 写入日志函数，信息类型
        /// </summary>
        /// <param name="message">消息</param>
        private static void writeInfo(string message)
        {
            //写到本地
            nonHistoryLog.Info(message);
            //写到日志服务器
            writetoFlume("info", message);
        }
        /// <summary>
        /// 写入日志函数，警告类型
        /// </summary>
        /// <param name="message">消息</param>
        private static void writeWarn(string message)
        {
            //写到本地
            nonHistoryLog.Warn(message);
            //写到日志服务器
            writetoFlume("warning", message);
        }
        /// <summary>
        /// 回调函数方法，测试用
        /// </summary>
        /// <param name="ar"></param>
        private void callBackMethod(IAsyncResult ia)
        {
            AsyncNonHistoryHandler anhh = (AsyncNonHistoryHandler)ia.AsyncState;
            anhh.EndInvoke(ia);
        }
        /// <summary>
        /// 写入日志函数，播放历史类型
        /// </summary>
        /// <param name="programName">节目名称</param>
        /// <param name="message">消息</param>
        /// <param name="authorName">作者名</param>
        /// <param name="duration">持续时间</param>
        /// <param name="path">路径</param>
        private static void writeHistoryToFlume(string programName, string message, string authorName, string duration, string path)
        {

            //写到日志服务器
            string postmessage = "节目名称:" + programName + ",播放信息:" + message + ",作者信息:" + authorName + ",持续时间:" + duration + ",播放路径:" + path;
            string poststring = flumestring + "\\\"messageType\\\":\\\"history\\\"," + "\\\"system\\\":\\\"播出系统\\\"," + "\\\"message\\\":\\\"" + realtime+postmessage + "\\\"}\"}]";
            asyntoflume(poststring);
        }

        /// <summary>
        /// 拼接json串，并将字符串提交到flume服务器
        /// </summary>
        /// <param name="type">提交日志类型</param>
        /// <param name="message">要提交的信息</param>
        public static void writetoFlume(string type, string message)
        {
            string poststring = flumestring + "\\\"messageType\\\":\\\"" + type + "\\\"," + "\\\"system\\\":\\\"播出系统\\\"," + "\\\"message\\\":\\\""+realtime + message + "\\\"}\"}]";
            asyntoflume(poststring);
        }

        /// <summary>
        /// 将信息发送到flume
        /// </summary>
        /// <param name="messsage">信息</param>

        public static void asyntoflume(string message)
        {
            //byte[] postbyte;
            Stream newStream = null;

            try
            {


                if (PingIp(pingip, 120) == true)
                {
                    WebRequest myHttpWebRequest = WebRequest.Create(ip);
                    myHttpWebRequest.Method = "POST";
                    UTF8Encoding encoding = new UTF8Encoding();
                    myHttpWebRequest.ContentType = "application/json;charset=UTF-8";
                    byte[] postbyte = encoding.GetBytes(message);
                    myHttpWebRequest.ContentLength = postbyte.Length;
                    newStream = myHttpWebRequest.GetRequestStream();//卡在了这步，直接跳出了
                    newStream.Write(postbyte, 0, postbyte.Length);
                   
                }
                else
                {

                    //写入本地错误日志
                    nonHistoryLog.Error("写入网络日志发生异常:网络连接异常");
                    //将要写入的信息生成.tmp临时文件
                    writeTmp(message);


                }
            }
            catch (Exception ex)
            {
                //写入本地错误日志
                nonHistoryLog.Error("写入网络日志发生异常:" + ex);

            }
            finally
            {
                if (newStream != null)
                {
                    newStream.Close();
                }

            }
        }
        /// <summary>
        /// 用于检查IP地址或域名是否可以使用TCP/IP协议访问(使用Ping命令),true表示Ping成功,false表示Ping失败 
        /// </summary>
        /// <param name="strIpOrDName">输入参数,表示IP地址或域名</param>
        /// <returns></returns>
        public static bool PingIp(string strIp, int intTimeout)//intTimeout ms
        {
            try
            {
                Ping objPingSender = new Ping();
                PingOptions objPinOptions = new PingOptions();
                objPinOptions.DontFragment = true;
                string data = "";
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                PingReply objPinReply = objPingSender.Send(strIp, intTimeout, buffer, objPinOptions);
                string strInfo = objPinReply.Status.ToString();
                // Console.WriteLine(strInfo);
                if (strInfo == "Success")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;


            }
        }
        /// <summary>
        /// 网络出现异常时，将要传递的信息存成临时文件
        /// </summary>
        /// <param name="message">未能写入日志服务器的信息</param>
        public static void writeTmp(string message)
        {
            string time = DateTime.Now.ToString("yyyyMMdd");

            //string newPath = System.IO.Path.Combine(activeDir, time);
            System.IO.Directory.CreateDirectory(activeDir);
            string path = "d:/logtmp/" + time + ".tmp";
            FileStream f = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter sw = new StreamWriter(f);
            sw.WriteLine(message);
            sw.Flush();
            sw.Close();
            f.Close();

        }

        /// <summary>
        /// 处理本地临时文件,处理前一天的临时文件 何时触发，待讨论
        /// </summary>
        /// <param name="dir">临时文件本地地址</param>
        public static void handletmp(string dir)
        {
            //string time1 = DateTime.Now.ToString("yyyyMM");
            //string time3 = null;
            //Int32 time2 = Convert.ToInt32(DateTime.Now.ToString("dd")) - 1;
            string posttmp = null;
            string date = DateTime.Now.ToString("yyyyMMdd");
            string filename = dir + "\\" + date  + ".tmp";
            FileInfo file = new FileInfo(filename);
            if (file.Exists)
            {

                try
                {

                    StreamReader sr = new StreamReader(filename, System.Text.Encoding.Default);//存在问题
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        posttmp = posttmp + line;
                        
                      
                    }
                    string poststring = flumestring + "\\\"messageType\\\":\\\tmp \\\"," + "\\\"system\\\":\\\"播出系统\\\"," + "\\\"message\\\":\\\"" + realtime + posttmp + "\\\"}\"}]";
                    asyntoflume(poststring);//可不可以把所有的tmp写在一条上传
                   // Console.WriteLine(posttmp);
                    //关闭文件流，删除文件成功
                     sr.Close();
                     deleteTmp(filename);
                }
                catch (IOException ex)
                {
                    nonHistoryLog.Error("处理临时文件发生异常:" + ex);
                }

            }


        }



        /// <summary>
        /// 删除临时文件
        /// </summary>
        /// <param name="fileUrl">临时文件地址</param>
        public static void deleteTmp(string fileUrl)
        {
            try
            {
                if (File.Exists(fileUrl))
                {
                    File.Delete(fileUrl);

                }
            }
            catch (Exception ex)
            {
                nonHistoryLog.Error("删除临时文件出现异常" + ex);
            }


        }
        /// <summary>
        /// 测试读取配置文件中的ip
        /// </summary>
        public static void getconfig()
        {
            //String str = ConfigurationManager.AppSettings["flumeIP"];
            Console.WriteLine(ip);
            Console.WriteLine(pingip);
            Console.WriteLine(activeDir);
        }
    }
}






