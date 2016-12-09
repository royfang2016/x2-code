﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace X2DisplayTest
{
    class Testlog
    {
        public Testlog()
        {
            if (!Directory.Exists(strPath_SummaryLog))
            {
                Directory.CreateDirectory(strPath_SummaryLog);
            }

            if (!Directory.Exists(strPath_UnitLog))
            {
                Directory.CreateDirectory(strPath_UnitLog);
            }

            if (!Directory.Exists(strCa310LogPath))
            {
                Directory.CreateDirectory(strCa310LogPath);
            }

            if (!Directory.Exists(strPath_TestLog))
            {
                Directory.CreateDirectory(strPath_TestLog);
            }

            if (!Directory.Exists(strPath_ColorimeterLog))
            {
                Directory.CreateDirectory(strPath_ColorimeterLog);
            }
        }
 
        // save the log to local for summary and detail information
        string strPath_TestLog = "C:\\vault\\LocalLog\\TestLog\\";
        string strPath_ColorimeterLog = "C:\\vault\\LocalLog\\ColorimeterLog\\";
        string strPath_UniformityLog = "C:\\vault\\LocalLog\\UniformityLog\\";

        string strPath_SummaryLog = "C:\\vault\\LocalLog\\";
        string strPath_UnitLog = "C:\\vault\\LocalLog\\UnitSpecialLog\\";
        string strName_SummaryLog = "J1J2_LCD Uniformity";
        string strHead_SummaryLog = "SerialNumber,Operator,TestStatus,Fail List Item,Start Time,Stop Time";
        string strValue_SummaryLog = "";
        string strUpperLimit_SummaryLog = "UpperLimit,,,,,";
        string strLowerLimit_SummaryLog = "LowerLimit,,,,,";
        string strUnits_SummaryLog = "Units,,,,,";
        string strUsingLogName = "";
        string str_FailList = "";
        static string vaule = "";

        // add the Sys ID for the unit
        string SYS_ID;
        bool bol_IsCheckOneDay = false;

        StreamWriter stream_SummaryLog;
        string str_DateDay;
        string Str_DateTime;
        string str_StartTestTime;
        string str_StopTestTime;

        private string strCa310LogPath = @"C:\vault\LocalLog\Ca310Log\";
        private StringBuilder uartData;
        private string sn;

        public string SerialNumber
        {
            get {
                return sn;
            }
            set {
                sn = value;
            }
        }

        private void InitCsvTitle(string fullFilePath, List<TestItem> allItems)
        {
            if (!File.Exists(fullFilePath))
            {
                string strHead_SummaryLog = "SerialNumber,Operator,TestStatus,Fail List Item,Start Time,Stop Time";
                string strUpperLimit_SummaryLog = "UpperLimit,,,,,";
                string strLowerLimit_SummaryLog = "LowerLimit,,,,,";
                string strUnits_SummaryLog = "Units,,,,,";
                foreach (TestItem testItem in allItems)
                {
                    foreach (TestNode node in testItem.TestNodes)
                    {
                        strHead_SummaryLog += "," + string.Format("{0}_{1}", testItem.TestName, node.NodeName);
                        strUpperLimit_SummaryLog += "," + node.Upper;
                        strLowerLimit_SummaryLog += "," + node.Lower;
                        strUnits_SummaryLog += "," + node.Unit;
                    }
                }

                using (StreamWriter wf = new StreamWriter(File.Create(fullFilePath)))
                {
                    wf.WriteLine(strHead_SummaryLog);
                    wf.WriteLine(strUpperLimit_SummaryLog);
                    wf.WriteLine(strLowerLimit_SummaryLog);
                    wf.WriteLine(strUnits_SummaryLog);
                    wf.Close();
                }
            }
        }

        private void InitCsvDataTitle(string fullFilePath, List<TestItem> allItems)
        {
            if (!File.Exists(fullFilePath))
            {
                string strHead_SummaryLog = "SerialNumber,Operator,TestStatus,Fail List Item,Start Time,Stop Time";
                string strUpperLimit_SummaryLog = "UpperLimit,,,,,";
                string strLowerLimit_SummaryLog = "LowerLimit,,,,,";
                string strUnits_SummaryLog = "Units,,,,,";
                foreach (TestItem testItem in allItems)
                {
                    foreach (TestNode node in testItem.TestNodes)
                    {
                        if (node.NodeName.Contains(@"luminance") == true || node.NodeName.Contains(@"mura") == true)
                        {
                            continue;
                        }
                        else
                        {
                            strHead_SummaryLog += "," + string.Format("{0}_{1}", testItem.TestName, node.NodeName);
                            strUpperLimit_SummaryLog += "," + node.Upper;
                            strLowerLimit_SummaryLog += "," + node.Lower;
                            strUnits_SummaryLog += "," + node.Unit;
                        }
                    }
                }

                using (StreamWriter wf = new StreamWriter(File.Create(fullFilePath)))
                {
                    wf.WriteLine(strHead_SummaryLog);
                    wf.WriteLine(strUpperLimit_SummaryLog);
                    wf.WriteLine(strLowerLimit_SummaryLog);
                    wf.WriteLine(strUnits_SummaryLog);
                    wf.Close();
                }
            }
        }
        public void writeCSVData(string sn, DateTime startTime, DateTime stopTime, string productType, List<TestItem> CameraItems,List<Dictionary<string,CIE1931Value>> Ca310Datas)//
        {
            string csvFileName = "X2DisplayTest_" +productType +"_"+ DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
            string strTestLogPath = strPath_TestLog + "\\" + productType;
            
            if (!Directory.Exists(strTestLogPath))
            {
                Directory.CreateDirectory(strTestLogPath);
            }

            string fullFilePath = Path.Combine(strTestLogPath, csvFileName);
            bool flagResult = true;
            string failItems = "";

            this.InitCsvDataTitle(fullFilePath, CameraItems);

            StringBuilder nodedata = new StringBuilder();
            StringBuilder data = new StringBuilder();

            if (string.IsNullOrEmpty(sn))
            {
                data.Append("xxxxxxxxxxxxxxxx");
            }
            else
            {
                data.Append(sn);
            }
      
            List<TestItem> TempData = new List<TestItem>();
            TempData = CameraItems;
            int index =0 ;
            foreach (Dictionary<string, CIE1931Value> CA310Items in Ca310Datas)
            {
                index = 0;
                foreach (string key in CA310Items.Keys)
                {
                    CIE1931Value cie = CA310Items[key];

                    TempData[index].TestNodes[3].Value = cie.x; 
                    TempData[index].TestNodes[4].Value = cie.y;
                    TempData[index].TestNodes[5].Value = cie.Y;
                    ++index;
                }
        

            }

            foreach (TestItem item in TempData)
            {
                foreach (TestNode node in item.TestNodes)
                {
                    if (node.NodeName.Equals(@"luminance") == false && node.NodeName.Equals(@"mura") == false)
                    {
                        bool flag = node.Run();
                        flagResult &= flag;

                        if (!flag)
                        {
                            failItems += string.Format("{0}_{1}", item.TestName, node.NodeName) + ";";
                        }
                        nodedata.AppendFormat("{0},", node.Value);
                    }

                }
            }

            data.AppendFormat(",adminstator,{0},{1},{2:yyyy-MM-dd HH:mm:ss},{3:yyyy-MM-dd HH:mm:ss},",
                (flagResult ? "PASS" : "FAIL"), failItems, startTime, stopTime);
            data.Append(nodedata.ToString());
            data.AppendLine();

            using (StreamWriter sw = new StreamWriter(fullFilePath, true))
            {
                sw.Write(data.ToString());
                sw.Flush();
                sw.Close();
            }

        }


        public void WriteCsv(string sn, DateTime startTime, DateTime stopTime, List<TestItem> allItems,string productType)
        {
            string csvFileName = "X2DisplayTest_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";

            string strColorimeterLogPath = strPath_ColorimeterLog + "\\" + productType;

            if (!Directory.Exists(strColorimeterLogPath))
            {
                Directory.CreateDirectory(strColorimeterLogPath);
            }

            string fullFilePath = Path.Combine(strColorimeterLogPath, csvFileName);
            bool flagResult = true;
            string failItems = "";

            this.InitCsvTitle(fullFilePath, allItems);

            StringBuilder nodedata = new StringBuilder();
            StringBuilder data = new StringBuilder();

            if (string.IsNullOrEmpty(sn)) {
                data.Append("xxxxxxxxxxxxxxxx");
            }
            else {
                data.Append(sn);
            }            

            foreach (TestItem item in allItems)
            {
                foreach (TestNode node in item.TestNodes)
                {
                    bool flag = node.Run();
                    flagResult &= flag;

                    if (!flag)
                    {
                        failItems += string.Format("{0}_{1}", item.TestName, node.NodeName) + ";";
                    }
                    nodedata.AppendFormat("{0},", node.Value);
                }
            }
            
            data.AppendFormat(",adminstator,{0},{1},{2:yyyy-MM-dd HH:mm:ss},{3:yyyy-MM-dd HH:mm:ss},",
                (flagResult ? "PASS" : "FAIL"), failItems, startTime, stopTime);
            data.Append(nodedata.ToString());
            data.AppendLine();

            using (StreamWriter sw = new StreamWriter(fullFilePath, true))
            {
                sw.Write(data.ToString());
                sw.Flush();
                sw.Close();
            }
        }

        public void AppendCamareVaule(string str)
        {
            vaule = vaule + str;
        }

        public void WriteCamareCsv(string sn, DateTime startTime, DateTime stopTime, string productType)
        {
            string csvFileName = "X2DisplayTest_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
            string strUniformityLog = strPath_UniformityLog + "\\" + productType;

            if (!Directory.Exists(strUniformityLog))
            {
                Directory.CreateDirectory(strUniformityLog);
            }

            string fullFilePath = Path.Combine(strUniformityLog, csvFileName);
           // bool flagResult = true;
           // string failItems = "";

            if (!File.Exists(fullFilePath))
            {
                string strHead_SummaryLog = "SerialNumber,Start Time,Stop Time,";
                string W = "W_1,W_2,W_3,W_4,W_5,W_6,W_7,W_8,W_9,W_10,W_11,W_12,W_13,";
                string R = "R_1,R_2,R_3,R_4,R_5,R_6,R_7,R_8,R_9,R_10,R_11,R_12,R_13,";
                string G = "G_1,G_2,G_3,G_4,G_5,G_6,G_7,G_8,G_9,G_10,G_11,G_12,G_13,";
                string B = "B_1,B_2,B_3,B_4,B_5,B_6,B_7,B_8,B_9,B_10,B_11,B_12,B_13,";
                string Black = "Black_1,Black_2,Black_3,Black_4,Black_5,Black_6,Black_7,Black_8,Black_9,Black_10,Black_11,Black_12,Black_13";
                strHead_SummaryLog = strHead_SummaryLog + W + R + G + B + Black;
                using (StreamWriter wf = new StreamWriter(File.Create(fullFilePath)))
                {
                    wf.WriteLine(strHead_SummaryLog);
                    wf.Close();
                }
            }

            StringBuilder nodedata = new StringBuilder();
            StringBuilder data = new StringBuilder();

            if (string.IsNullOrEmpty(sn))
            {
                data.Append("xxxxxxxxxxxxxxxx");
            }
            else
            {
                data.Append(sn);
            }

            data.AppendFormat("{0},{1:yyyy-MM-dd HH:mm:ss},{2:yyyy-MM-dd HH:mm:ss}{3}", sn, startTime, stopTime, vaule);
            data.Append(nodedata.ToString());
            data.AppendLine();

            using (StreamWriter sw = new StreamWriter(fullFilePath, true))
            {
                sw.Write(data.ToString());
                sw.Flush();
                sw.Close();
                vaule = "";
            }
        }

        public void WriteUartLog(string data)
        {
            if (uartData == null)
            {
                uartData = new StringBuilder();
            }

            uartData.AppendFormat("[{0:yyyy-MM-dd HH:mm:ss}]: {1}\n", DateTime.Now, data);
        }

        public void UartFlush()
        {
            string fileName = string.Format("{0}_{1:yyyy-MM-dd_HHmmss}.txt", sn, DateTime.Now);

            using (StreamWriter sw = new StreamWriter(Path.Combine(strPath_UnitLog, fileName)))
            {
                sw.WriteLine(uartData.ToString());
                sw.Flush();
                sw.Close();
            }
        }

        public void WriteCa310Log(string sn, Dictionary<string, CIE1931Value> items)
        {
            string csvFileName = "X2DisplayTest_Ca310_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
            string fullFilePath = Path.Combine(strCa310LogPath, csvFileName);

            if (!File.Exists(fullFilePath))
            {
                FileStream stream = File.Create(fullFilePath);
                stream.Close();
            }

            StringBuilder sbStr = new StringBuilder();

            foreach (string key in items.Keys)
            {
                CIE1931Value cie = items[key];
                sbStr.AppendFormat("{0},{1},{2},{3}\r\n", key, cie.x, cie.y, cie.Y);
            }

            using (StreamWriter sw = new StreamWriter(fullFilePath, true))
            {
                sw.WriteLine(string.Format("{0},x,y,Y", sn));
                sw.WriteLine(sbStr.ToString());
                sw.Flush();
                sw.Close();
            }
        }

        public void writecsv(string dir, string filename, double[,] matrix)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            
            StreamWriter fileLV = File.AppendText(filename);

            int rowNum = matrix.GetLength(0);
            int columnNum = matrix.GetLength(1);
            for (int i = 0; i < rowNum; i++)
            {
                for (int j = 0; j < columnNum; j++)
                {
                    if (j == columnNum - 1)
                    {
                        fileLV.Write(matrix[j, i].ToString());
                    }
                    else
                    {
                        fileLV.Write(matrix[j, i].ToString() + "\t");
                    }
                }

                fileLV.Write("\r\n");
            }

            fileLV.Flush();
            fileLV.Close();

        }

        public void AddLocalAndPDCALog(string str_ItemName, string str_TestValue, 
            string str_Result, string str_UpperLimit, string str_LowerLimit, string str_Units)
        {
            string str_UpperLimit_Tmp = str_UpperLimit;
            string str_LowerLimit_Tmp = str_LowerLimit;
            string str_Units_Tmp = str_Units;
            //string str_FailInfor_Tmp = str_FailInfor;

            if (str_UpperLimit_Tmp == string.Empty)
            {
                str_UpperLimit_Tmp = "NA";
            }

            if (str_LowerLimit_Tmp == string.Empty)
            {
                str_LowerLimit_Tmp = "NA";
            }

            if (str_Units_Tmp == string.Empty)
            {
                str_Units_Tmp = "NA";
            }

            //if (str_FailInfor_Tmp == string.Empty)
            //{
            //    str_FailInfor_Tmp = "Test Failed.";
            //}

            //list_ItemName.Add(str_ItemName);
            //list_UpperLimit.Add(str_UpperLimit);
            //list_LowerLimit.Add(str_LowerLimit);
            //list_Units.Add(str_Units);
            ////list_FailInfor.Add(str_FailInfor);


            if (string.Compare(str_Result, "FAIL", true) == 0)
            {
                str_FailList += str_ItemName + ";";
            }

            strValue_SummaryLog += "," + str_TestValue;
            strHead_SummaryLog += "," + str_ItemName;
            strUpperLimit_SummaryLog += "," + str_UpperLimit;
            strLowerLimit_SummaryLog += "," + str_LowerLimit;
            strUnits_SummaryLog += "," + str_Units;

            //     ip_Pudding.AddTestItem(str_ItemName, "", "", str_LowerLimit_Tmp, str_UpperLimit_Tmp, str_Units_Tmp, str_Result, str_TestValue, "The Valule is out of spec.", "0");

        }

        ///
        public void CheckLocalLogForOneDay()
        {
            if (bol_IsCheckOneDay)
            {
                return;
            }

            DateTime date_Today = DateTime.Now;
            // get the date of today
            string str_Date = string.Format("{0:yyyyMMdd}", date_Today);
            string str_tmp = strHead_SummaryLog + "\n" + strUpperLimit_SummaryLog + "\n" + strLowerLimit_SummaryLog + "\n" + strUnits_SummaryLog;

            // get the log list
            DirectoryInfo diInfo = new DirectoryInfo(strPath_SummaryLog);
            FileInfo[] files = diInfo.GetFiles();

            foreach (FileInfo file in files)
            {
                DateTime date_LastWriteTime = file.LastWriteTime;
                string str_data_LastWriteTime = string.Format("{0:yyyyMMdd}", date_LastWriteTime);

                // the log should be 24h
                if (str_data_LastWriteTime.CompareTo(str_Date) == 0)
                {
                    StreamReader stream_Reader = new StreamReader(strPath_SummaryLog + file);
                    string str_head = stream_Reader.ReadLine() + "\n";
                    str_head += stream_Reader.ReadLine() + "\n";
                    str_head += stream_Reader.ReadLine() + "\n";
                    str_head += stream_Reader.ReadLine() + "\n";
                    str_head += stream_Reader.ReadLine();
                    stream_Reader.Close();

                    str_head = str_head.Replace("\n", " ");
                    string str_newhead_tmp = str_tmp.Replace("\n", " ");

                    if (str_head.CompareTo(str_newhead_tmp) == 0)
                    {
                        strUsingLogName = strPath_SummaryLog + file;
                        stream_SummaryLog = File.AppendText(strPath_SummaryLog + file);
                        bol_IsCheckOneDay = true;
                        break;
                    }
                }
            }

            if (!bol_IsCheckOneDay)
            {
                if (stream_SummaryLog != null)
                {
                    stream_SummaryLog.Flush();
                    stream_SummaryLog.Close();
                }

                strUsingLogName = strPath_SummaryLog + strName_SummaryLog + "_" + string.Format("{0:yyyyMMdd}" + "{0:HHmmss}", date_Today, date_Today) + ".csv";
                stream_SummaryLog = File.AppendText(strUsingLogName);
                bol_IsCheckOneDay = true;
                stream_SummaryLog.Write(str_tmp);
                stream_SummaryLog.Flush();
            }

            str_DateDay = str_Date;
        }


        public void FinishLocalLog()
        {
            DateTime testTime = DateTime.Now;
            //Str_DateTime = string.Format("{0:yyyyMMdd}" + "_" + "{0:HHmmss}", testTime, testTime);
            str_StopTestTime = string.Format("{0:yyyyMMdd}" + "{0:HHmmss}", testTime, testTime);
            string str_TestStatus = "PASS";

            string str_tmp = string.Format("{0:yyyyMMdd}", testTime);

            if (string.Compare(str_DateDay, str_tmp, true) != 0)// check the 24h
            {
                bol_IsCheckOneDay = false;
            }

            if (!bol_IsCheckOneDay)
            {
                CheckLocalLogForOneDay();
            }

            if (SYS_ID == string.Empty)
            {
                SYS_ID = "NO_SN";
                str_TestStatus = "FAIL";
                str_FailList += "SN;";
            }

            if (str_FailList != string.Empty)
            {
                str_TestStatus = "FAIL";
            }

            strValue_SummaryLog = "\n" + SYS_ID + "," + str_TestStatus + "," + str_FailList + "," + str_StartTestTime + "," + str_StopTestTime + strValue_SummaryLog;
            stream_SummaryLog.Write(strValue_SummaryLog);
            stream_SummaryLog.Flush();
            stream_SummaryLog.Close();

            stream_SummaryLog = File.AppendText(strUsingLogName);

            strValue_SummaryLog = "";
            strHead_SummaryLog = "";
            strUpperLimit_SummaryLog = "";
            strLowerLimit_SummaryLog = "";
            strUnits_SummaryLog = "";
            str_FailList = "";

            //         ip_Pudding.commitInstantPudding(true);
        }

    }
}
