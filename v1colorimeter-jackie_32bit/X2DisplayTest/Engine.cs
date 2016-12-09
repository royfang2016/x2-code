using System;
using System.Collections.Generic;
using System.Threading;
using System.Drawing;
using AForge;
using DUTclass;
using System.IO;


namespace X2DisplayTest
{
    public delegate void DataDelegate(object sender, DataChangeEventArgs args);
    public delegate void TableViewDelegate(object sender, TableViewEventArgs args);

    public class TableViewEventArgs
    {
        public string CurrentDevice { get; set; }
        public int Index { get; set; }
        public List<TestItem> Items { get; set; }
    }

    public class DataChangeEventArgs
    {
        public string SerialNumber { get; set; }
        public string StatusInfo { get; set; }
        public string CCDStatusInfo { get; set; }
        public string PassOrFail { get; set; }
        public double CCDTemperature { get; set; }
        public TimeSpan Uptime { get; set; }
        public Bitmap Image { get; set; }

        public DataChangeEventArgs()
        {
            PassOrFail = StatusInfo = "READY";
            CCDStatusInfo = "";
            CCDTemperature = 0;
        }
    }

    public class Engine
    {
        public Engine(Config config)
        {
            this.config = config;
            this.colorimeter = new Colorimeter();
            this.xml = new Xml(this.config.ScriptName);

            if (!this.config.IsSimulation)
            {
                this.fixture = new Fixture(this.config.FixturePortName);
                //this.ca310Pipe = new Ca310Pipe(System.Windows.Forms.Application.StartupPath);
                IDevice intergrate = new IntegratingSphere(this.fixture, this.config.LCP3005PortName);
                DevManage.Instance.AddDevice(fixture);
                DevManage.Instance.AddDevice(intergrate);
            }

            dut = (DUT)Activator.CreateInstance(Type.GetType("DUTclass." + this.config.ProductType));
            mode = (TestMode)Enum.Parse(typeof(TestMode), this.config.TestMode);

            ip = new imagingpipeline();
            args = new DataChangeEventArgs();
            tableArgs = new TableViewEventArgs();
            tableArgs.Items = xml.Items;

            log = new Testlog();
            SerialNumber = "";

            if (!System.IO.Directory.Exists(IMAGE_SAVE_PATH))
            {
                System.IO.Directory.CreateDirectory(IMAGE_SAVE_PATH);
            }
        }

        private readonly string IMAGE_SAVE_PATH = "D:\\x2_images\\";

        public event DataDelegate dataChange;
        private DataChangeEventArgs args;

        public event TableViewDelegate tableDataChange;
        private TableViewEventArgs tableArgs;

        private bool flagFan;
        private bool flagExit;
        private bool flagCavus;

        private float redWeight = 0.72f;
        private float greenWeight = 0.18f;
        private float blueWeight = 0.1f;

        private float optimalExp;
        string dataPath = @"C:\eBook_Test\";
        private Colorimeter colorimeter;
        private Config config;

        private Xml xml;
        public Xml XmlManage
        {
            get { return xml; }
        }

        private Testlog log;
        private Ca310Pipe ca310Pipe;

        private KonicaCa310 ca310Hanle;

        private Fixture fixture;
        private imagingpipeline ip;

        List<Dictionary<string, CIE1931Value>> CA310Datas = new List<Dictionary<string, CIE1931Value>>();
        //private Imageprocess.ImagingPipeline pipeline;

        public int SNLength
        {
            get
            {
                if (Dut is DUTclass.Hodor)
                {
                    return 16;
                }
                else if (Dut is DUTclass.Bran)
                {
                    return 16;
                }

                return 0;
            }
        }

        //dut setup
        private DUT dut;
        public DUT Dut
        {
            get
            {
                return dut;
            }
            set
            {
                dut = value;
            }
        }

        private int testpoints;
        public int TestPoints
        {
            get
            {
                if (dut is DUTclass.Bran)
                {
                    testpoints = 9;
                }
                else if (dut is DUTclass.Hodor)
                {
                    testpoints = 13;
                }

                return testpoints;
            }
        }

        // test mode
        private TestMode mode;
        public TestMode TestMode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
            }
        }

        public List<TestItem> Items
        {
            get
            {
                if (this.xml == null)
                {
                    this.xml = new Xml(this.config.ScriptName);
                }

                return this.xml.Items;
            }
        }

        public bool IsCa310Test { get; private set; }
        public bool IsDutReady { get; private set; }
        public bool IsShopFlowReady { get; private set; }
        public string SerialNumber { private get; set; }
        public bool TestResult { get; private set; }

        private Thread tdBlock;
        public int testNum;

        public void Initilazie()
        {
            new Action(delegate()
            {
                try
                {
                    if (!colorimeter.Connect())
                    {
                        args.StatusInfo = "No Camera";
                    }
                    colorimeter.Shutter = 60;

                    DateTime timezero = DateTime.Now;

                    while (!flagExit)
                    {
                        if (dataChange != null)
                        {
                            args.Uptime = DateTime.Now.Subtract(timezero);

                            if (colorimeter.Temperature < 20 && args.Uptime.Hours < 24)
                            {
                                if (!this.config.IsSimulation)
                                {
                                    if (flagFan)
                                    {
                                        fixture.FanOff();
                                        flagFan = !flagFan;
                                    }
                                }
                                args.CCDStatusInfo = "OK";
                            }
                            else if (colorimeter.Temperature < 50 && args.Uptime.Hours < 24)
                            {
                                if (!this.config.IsSimulation)
                                {
                                    if (flagFan)
                                    {
                                        fixture.FanOn();
                                        flagFan = !flagFan;
                                    }
                                }
                                args.CCDStatusInfo = "Warm CCD";
                            }
                            args.CCDTemperature = colorimeter.Temperature;
                            dataChange.Invoke(this, args);
                        }
                        System.Threading.Thread.Sleep(100);
                    }
                }
                catch (Exception e)
                {
                    args.StatusInfo = e.Message;
                    dataChange.Invoke(this, args);
                    Console.WriteLine(e.StackTrace.ToString());
                }
            }).BeginInvoke(null, null);
        }

        public void VideoCavus(System.Windows.Forms.PictureBox cavus)
        {
            colorimeter.SetVideoCavaus(cavus);
            flagCavus = true;
        }

        public void Start()
        {
            tdBlock = new Thread(RunSequence)
            {
                IsBackground = true
            };
            tdBlock.Start();
        }

        public void Stop()
        {
            testNum = 0;
            this.SerialNumber = "";
            Thread.Sleep(500);

            if (tdBlock != null)
            {
                tdBlock.Abort();
                tdBlock = null;
            }
        }

        public void Exit()
        {
            flagExit = true;
            Stop();
            dut.Dispose();

            if (!this.config.IsSimulation)
            {
                // fixture
                fixture.Exit();
                fixture = null;

                // camera
                colorimeter.Disconnect();
                colorimeter = null;

                ca310Pipe.Disconnect();
                ca310Pipe = null;
            }
        }

        private bool CheckShopFloor()
        {
            bool flag = false;
            int status = -1;
            DateTime timeNow = DateTime.Now;

            do
            {
                // do something to check shopfloor
                if (this.config.IsOnlineShopfloor)
                {
                    SFC.SFCInit();
                    status = SFC.ReportStatus(this.SerialNumber, 1);

                    if (status == 0)
                    {
                        flag = true;
                        break;
                    }
                }
                else
                {
                    flag = true;
                    break;
                }
            }
            while (DateTime.Now.Subtract(timeNow).TotalMilliseconds < 5000);

            return flag;
        }

        private void UploadItemDataToSFC(TestItem testItem, string testDevice)   /////upSFC
        {
            string path = @"C:\eBook_Test\1_WIP_INFO.txt";

            using (StreamReader sw = new StreamReader(path))
            {
                sw.ReadLine();
                sw.ReadLine();
                string readTemp = sw.ReadLine().Trim();
                //string readTemp = sw.ReadToEnd().ToString();
                sw.Close();

                int wipIndex = readTemp.IndexOf(@"WIP_ID=");
                if (readTemp.Contains("WIP_ID="))
                {
                    string wipID = readTemp.Substring(7);
                    dataPath = @"C:\eBook_Test\" + wipID + ".txt";
                }
                if (File.Exists(dataPath) == false)
                {
                    FileStream stream = File.Create(dataPath);
                    stream.Close();
                }
            }

            int indexStart = 0, indexEnd = testItem.TestNodes.Count;
            string name = null;
            string passfail = "PASS", upperStr = "", lowerStr = "";

            if (testDevice == "Camera")
            {
                indexStart = 1;
                indexEnd = 2;
            }
            else if (testDevice == "Ca310")
            {
                indexStart = 3;
                indexEnd = testItem.TestNodes.Count;
            }


            for (int i = indexStart; i < indexEnd; i++)
            {
                ++testNum;
                name = string.Format("{0}_{1}[{2}]", testItem.TestName, testItem.TestNodes[i].NodeName, testDevice);
                passfail = testItem.TestNodes[i].Result ? "PASS" : "FAIL";
                upperStr = (testItem.TestNodes[i].Upper == double.NaN) ? "_" : testItem.TestNodes[i].Upper.ToString();
                lowerStr = (testItem.TestNodes[i].Lower == double.NaN) ? "_" : testItem.TestNodes[i].Lower.ToString();

                // int nFlag = SFC.AddTestLog(1, (uint)i, name, upperStr, lowerStr, testItem.TestNodes[i].Value.ToString(), passfail);
                //int nFlag = SFC.AddTestLog(1, (uint)testNum, name, testItem.TestNodes[i].Value.ToString(), upperStr, lowerStr, passfail);
                //if (nFlag != 0)
                //{
                //    args.StatusInfo = "Fail to upload SFC.";
                //}

                string line = string.Format("TEST_ITEM_{0:D2}={1}^{2}^{3}^{4}^{5}^{6}", testNum, name,
                          name, testItem.TestNodes[i].Value.ToString(), upperStr, lowerStr, passfail);
                // string filePath = @"C:\eBook_Test\1_WIP_INFO.txt";


                using (StreamWriter sw = new StreamWriter(dataPath, true))
                {
                    sw.WriteLine(line.ToString());
                    sw.Flush();
                    sw.Close();
                }




            }

            /*
for (int i = indexStart; i < indexEnd; i++)
{
    name = string.Format("{0}_{1}[{2}]", testItem.TestName, testItem.TestNodes[0].NodeName, testDevice);
    passfail = testItem.TestNodes[i].Result ? "PASS" : "FAIL";
    upperStr = (testItem.TestNodes[1].Upper == double.NaN) ? "_" : testItem.TestNodes[1].Upper.ToString();
    lowerStr = (testItem.TestNodes[2].Lower == double.NaN) ? "_" : testItem.TestNodes[2].Lower.ToString();

   // int nFlag = SFC.AddTestLog(1, (uint)i, name, upperStr, lowerStr, testItem.TestNodes[i].Value.ToString(), passfail);

   // if (nFlag != 0)
    {
        args.StatusInfo = "Fail to upload SFC.";
    }
}*/

        }
        /*
          private void InitCa310()
          {
              #region Init Ca310
              if (mode == TestMode.Ca310)
              {
                  if (ca310Pipe == null)
                  {
                      ca310Pipe = new Ca310Pipe(System.Windows.Forms.Application.StartupPath);
                      ca310Hanle = new KonicaCa310();
                      args.StatusInfo = "Initilaze Ca310 device.";

                      if (!ca310Pipe.Connect())
                      {
                          args.StatusInfo = ca310Pipe.ErrorMessage;
                      }
                      else
                      {
                          args.StatusInfo = "Ca310 has Connected.";
                          ca310Pipe.ResetZero();
                      }
                  }
              }
              else
              {
                  if (ca310Pipe != null)
                  {
                      ca310Pipe.Disconnect();
                      ca310Pipe = null;
                  }
              }
              #endregion
          }
          */

        private void InitCa310()
        {
            #region Init Ca310
            if (mode == TestMode.Ca310)
            {
                if (ca310Hanle == null)
                {
                    ca310Hanle = new KonicaCa310();
                    args.StatusInfo = "Initilaze Ca310 device.";
                    ca310Hanle.Initiaze();
                    if (ca310Hanle.ErrorMessage != "")
                    {
                        args.StatusInfo = ca310Hanle.ErrorMessage;
                    }
                    else
                    {
                        args.StatusInfo = "Ca310 has Connected.";
                        ca310Hanle.Zero();
                    }
                }
            }
            else
            {
                if (ca310Hanle != null)
                {
                    ca310Hanle = null;
                }
            }
            #endregion
        }

        private void RunCa310Test()
        {
            if (mode == TestMode.Ca310)
            {
                int index = 0;
                const string deviceName = "Ca310";
                Dictionary<string, CIE1931Value> items = new Dictionary<string, CIE1931Value>();

                if (!this.config.IsSimulation)
                {
                    fixture.RotateOn();
                }

                for (int i = 0; i < xml.Items.Count; i++)
                {
                    TestItem testItem = xml.Items[i];
                    log.WriteUartLog(string.Format("Ca310Mode - Set panel to {0}\r\n", testItem.TestName));

                    if (dut.ChangePanelColor(testItem.RGB.R, testItem.RGB.G, testItem.RGB.B))
                    {
                        Thread.Sleep(3000);
                        CIE1931Value cie = ca310Hanle.GetCa310Data();
                        if (ca310Hanle.ErrorMessage != "")
                        {
                            args.StatusInfo = ca310Hanle.ErrorMessage;
                        }
                        log.WriteUartLog(string.Format("Ca310Mode - CIE1931xyY: {0}\r\n", cie.ToString()));

                        testItem.TestNodes[3].Value = cie.x;
                        testItem.TestNodes[4].Value = cie.y;
                        testItem.TestNodes[5].Value = cie.Y;
                        items.Add(testItem.TestName, cie.Copy());


                        TestResult &= testItem.RunCa310();
                        // flush UI
                        if (tableDataChange != null)
                        {
                            tableArgs.CurrentDevice = deviceName;
                            tableArgs.Index = index++;
                            tableDataChange(this, tableArgs);
                        }

                        if (this.config.IsOnlineShopfloor && cie.x > 0) // debug
                        {
                            UploadItemDataToSFC(testItem, deviceName);
                        }
                    }
                    else
                    {
                        args.StatusInfo = string.Format("Can't set panel color to {0}\r\n", testItem.TestName);
                        break;
                    }
                }

                if (!this.config.IsSimulation)
                {
                    fixture.RotateOff();
                }
                CA310Datas.Add(items);
                log.WriteCa310Log(SerialNumber, items);
            }
        }

        private void RunSequence()
        {
            do
            {
                this.InitCa310();
                this.RunSignalSequence();
            }
            while (!flagExit);
        }

        private void RunSignalSequence()
        {
            //args.PassOrFail = "READY";
            testNum = 0;
            args.SerialNumber = SerialNumber = "";

            if (!this.config.IsSimulation)
            {
                //args.PassOrFail = "READY";
                //if (this.config.IsScanSerialNumber) {
                //    args.StatusInfo = "Checking SN, please type in 16 digit SN.扫描SN";
                //    while (SerialNumber.Length != SNLength) { Thread.Sleep(100); }
                //}
                args.StatusInfo = "Waitting double-satrt pressed...按双启";
                fixture.CheckDoubleStart();
                fixture.BatteryOn();
            }

            if (mode != TestMode.Manual)
            {
                args.StatusInfo = "Checking DUT";
                while (!dut.CheckDUT()) { Thread.Sleep(100); } // check dut
                log.WriteUartLog(string.Format("DUT connected, DeviceID: {0}\r\n", dut.DeviceID));
            }
            else
            {
                args.StatusInfo = "DUT connected.";
                log.WriteUartLog("DUT connected.");
            }

            IsDutReady = true;

            if (!this.config.IsScanSerialNumber)
            {
                args.StatusInfo = "Checking SN";
                do
                {
                    SerialNumber = dut.GetSerialNumber();
                    Thread.Sleep(100);
                }
                while (SerialNumber.Length != SNLength);
                args.SerialNumber = SerialNumber;
            }
            else
            {
                if (this.config.IsScanSerialNumber)
                {
                    args.StatusInfo = "Checking SN, please type in 16 digit SN.扫描SN";
                    while (SerialNumber.Length != SNLength) { Thread.Sleep(100); }
                }
            }


            string strProduct = SerialNumber.Substring(6, 2);

            if (strProduct.Equals("78"))
            {
                if (config.ProductType.Equals("Bran") == false)
                {
                    System.Windows.Forms.MessageBox.Show("请检查产品类型是否和选定类型一致！");
                }
            }
            else if (strProduct.Equals("79"))
            {
                if (config.ProductType.Equals("Hodor") == false)
                {
                    System.Windows.Forms.MessageBox.Show("请检查产品类型是否和选定类型一致！");
                }
            }
            log.SerialNumber = SerialNumber;
            log.WriteUartLog(string.Format("Serial number: {0}\r\n", SerialNumber));
            args.StatusInfo = string.Format("Serial number: {0}", SerialNumber);

            // check shopfloor
            args.StatusInfo = "Checking Shopfloor";
            IsShopFlowReady = this.CheckShopFloor();

            if (!IsShopFlowReady)
            {
                args.StatusInfo = "Shopfloor system is not working";
                log.WriteUartLog("Shopfloor system is not working.\r\n");
                System.Windows.Forms.MessageBox.Show("请检查SFC是否打开或者产品是否已经过站！");
            }
            else
            {
                log.WriteUartLog("Shopfloor has connected.\r\n");
                args.StatusInfo = "Testing...";
                args.PassOrFail = "TESTING";

                if (tableDataChange != null)
                {
                    string[] devNames = new string[] { "Camera", "Ca310" };

                    foreach (string dev in devNames)
                    {
                        tableArgs.Index = -1;
                        foreach (TestItem item in tableArgs.Items)
                        {
                            item.TestNodes[0].Value = 0;
                            item.TestNodes[1].Value = 0;
                            item.TestNodes[2].Value = 0;
                            item.TestNodes[3].Value = 0;
                            item.TestNodes[4].Value = 0;
                            item.TestNodes[5].Value = 0;
                            tableArgs.Index++;
                            tableArgs.CurrentDevice = dev;
                            tableDataChange(this, tableArgs);
                        }
                    }
                }

                // run Ca310 if the mode is Ca310Mode
                CA310Datas.Clear();
                TestResult = true;
                this.RunCa310Test();

                DateTime startTime = DateTime.Now, stopTime;
                List<IntPoint> ptCorners = new List<IntPoint>();


                int index = 0;
                const string deviceName = "Camera";

                foreach (TestItem testItem in xml.Items)
                {
                    log.WriteUartLog(string.Format("Set panel to {0}\r\n", testItem.TestName));

                    bool flag = false;

                    if (mode == TestMode.Manual)
                    {
                        if (flagCavus) { colorimeter.PlayVideo(); }
                        System.Windows.Forms.MessageBox.Show(string.Format("Please set panel to \"{0}\"", testItem.TestName));
                        flag = true;
                        if (flagCavus) { colorimeter.StopVideo(); Thread.Sleep(1000); }
                    }
                    else
                    {
                        args.StatusInfo = string.Format("Set panel to \"{0}\"", testItem.TestName);
                        flag = dut.ChangePanelColor(testItem.RGB.R, testItem.RGB.G, testItem.RGB.B);
                    }

                    if (flag)
                    {
                        if (mode != TestMode.Manual)
                        {
                            Thread.Sleep(3000);
                        }

                        colorimeter.Shutter = (float)testItem.Exposure;
                        colorimeter.ConfigCamera(Color.FromName(testItem.TestName));
                        Bitmap bitmap = new Bitmap(colorimeter.GrabImage());
                        Bitmap bmpDisplay = null;

                        if (testItem.TestName != "White" && testItem.TestName != "Black")
                        {
                            colorimeter.RefushShutter(Color.FromName(testItem.TestName));
                            bmpDisplay = new Bitmap(colorimeter.GrabImage());
                            args.Image = new Bitmap(bmpDisplay);
                        }
                        else
                        {
                            args.Image = new Bitmap(bitmap);
                        }

                        dataChange.Invoke(this, args);
                        TestResult &= this.RunDisplayTest(testItem, bitmap, bmpDisplay, ref ptCorners);

                        if (ptCorners == null || ptCorners.Count == 0) { break; }

                        if (this.config.IsOnlineShopfloor)
                        {
                            UploadItemDataToSFC(testItem, deviceName);
                        }

                        // flush UI
                        if (tableDataChange != null)
                        {
                            tableArgs.CurrentDevice = deviceName;
                            tableArgs.Index = index++;
                            tableDataChange(this, tableArgs);
                        }
                    }
                    else
                    {
                        args.StatusInfo = string.Format("Can't set panel color to {0}\r\n", testItem.TestName);
                        //dataChange.Invoke(this, args);
                        break;
                    }
                }
                args.StatusInfo = "Test finished.";
                args.PassOrFail = TestResult ? "PASS" : "FAIL";
                dataChange(this, args);
                log.WriteUartLog(string.Format("Test result is {0}\r\n", (TestResult ? "PASS" : "FAIL")));
                log.UartFlush();

                stopTime = DateTime.Now;
                log.WriteCsv(SerialNumber, startTime, stopTime, xml.Items, this.config.ProductType);
                log.writeCSVData(SerialNumber, startTime, stopTime, this.config.ProductType, xml.Items, CA310Datas);
                log.WriteCamareCsv(SerialNumber, startTime, stopTime, this.config.ProductType);

                if (this.config.IsOnlineShopfloor)
                {
                    SFC.CreateResultFile(1, TestResult ? "PASS" : "FAIL");
                }

                if (!this.config.IsSimulation)
                {
                    fixture.HoldOut();
                }


            }
        }

        private bool RunDisplayTest(TestItem testItem, Bitmap bitmap, Bitmap bmpDisp, ref List<IntPoint> ptCorners)
        {
            string imageName = string.Format("{0}{1}_{2:yyyyMMddHHmmss}_{3}.bmp", IMAGE_SAVE_PATH, SerialNumber, DateTime.Now, testItem.TestName);

            if (testItem.RGB == Color.FromArgb(255, 255, 255))
            {
                //Process Image to 1bpp to increase SNR 
                Bitmap m_orig = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                // only support the 32bppArgb for Aforge Blob Counter
                Bitmap processbmp = m_orig.Clone(new Rectangle(0, 0, m_orig.Width, m_orig.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                ip.GetDisplayCornerfrombmp(processbmp, out ptCorners);
            }

            if (ptCorners == null)
            {
                return false;
            }

            // save original image   
            // bitmap: the first bitmap, bmpDisp: the second bitmap.
            if (bmpDisp == null)
            {
                bmpDisp = bitmap;
            }
            bmpDisp.Save(imageName);

            // cropping screen image
            Bitmap srcimg = new Bitmap(System.Drawing.Image.FromFile(imageName, true));
            Bitmap updateimg = CroppingImage((Bitmap)srcimg.Clone(), ptCorners);
            args.Image = updateimg;
            dataChange.Invoke(this, args);

            // 截取区域图像
            Bitmap cropimg = ip.croppedimage(srcimg, ptCorners, dut.ui_width, dut.ui_height);
            cropimg.Save(imageName + "_cropped.bmp");
            args.Image = new Bitmap(cropimg);
            dataChange.Invoke(this, args);

            Bitmap bmpCroping = null;

            if (bmpDisp != bitmap)
            {
                bmpCroping = ip.croppedimage(bitmap, ptCorners, dut.ui_width, dut.ui_height);
            }
            else
            {
                bmpCroping = cropimg;
            }

            // anaylse
            ColorimeterResult colorimeterRst = new ColorimeterResult(bmpCroping, cropimg);
            colorimeterRst.Analysis(ref testItem, dut);

            //if (this.config.IsOnlineShopfloor)
            //{
            //    UploadItemDataToSFC(testItem, "Camera");
            //}
            this.DrawZone(cropimg, testItem.TestName);

            return testItem.RunUnifAndMura(); //testItem.Run();
        }

        private Graphics ZoneImage(Graphics g, List<IntPoint> cornerPoints)
        {
            List<System.Drawing.Point> Points = new List<System.Drawing.Point>();
            foreach (var point in cornerPoints)
            {
                Points.Add(new System.Drawing.Point(point.X, point.Y));
            }
            g.DrawPolygon(new Pen(Color.Red, 1.0f), Points.ToArray());
            return g;
        }

        private void DrawZone(Bitmap binImage, string panelName)
        {
            int productType = 0;

            if (dut is DUTclass.Bran)
            {
                productType = 0;
            }
            else if (dut is DUTclass.Hodor)
            {
                productType = 1;
            }

            string imageName = string.Format("{0}{1}{2:yyyyMMddHHmmss}_{3}.bmp", IMAGE_SAVE_PATH, SerialNumber, DateTime.Now, panelName);
            zoneresult zr = new zoneresult();
            Graphics g = Graphics.FromImage(binImage);
            double[, ,] XYZ = ip.bmp2rgb(binImage);

            zr.clear();

            for (int i = 1; i < (this.TestPoints + 1); i++)
            {
                // get corner coordinates
                List<IntPoint> flagPoints = zr.zonecorners(productType, i, 10, XYZ);
                // zone image
                g = ZoneImage(g, flagPoints);
                binImage.Save(IMAGE_SAVE_PATH + i.ToString() + "_" + panelName.ToString() + "_bin_zone.bmp");
                flagPoints.Clear();
            }

            binImage.Save(imageName + "_bin_zone1-" + this.TestPoints + ".bmp");
            //refreshtestimage(binImage, picturebox_test);
            args.Image = new Bitmap(binImage);
            dataChange.Invoke(this, args);
            g.Dispose();
        }

        private Bitmap CroppingImage(Bitmap srcimg, List<IntPoint> cornerPoints)
        {
            Graphics g = Graphics.FromImage(srcimg);
            List<System.Drawing.Point> Points = new List<System.Drawing.Point>();

            if (cornerPoints == null || cornerPoints.Count == 0) { return srcimg; }

            foreach (var point in cornerPoints)
            {
                Points.Add(new System.Drawing.Point(point.X, point.Y));
            }
            g.DrawPolygon(new Pen(Color.Red, 15.0f), Points.ToArray());
            srcimg.Save(IMAGE_SAVE_PATH + SerialNumber + DateTime.Now.ToString("yyyyMMddHHmmss") + "_cropping.bmp");
            g.Dispose();
            return srcimg;
        }

        public void Video(System.Windows.Forms.PictureBox cavaus, bool alive)
        {
            if (alive)
            {
                colorimeter.SetVideoCavaus(cavaus);
                colorimeter.PlayVideo();
            }
            else
            {
                colorimeter.StopVideo();
            }
        }

        public int ShowColorimeterDialog(bool isModalMode)
        {
            int result = 0;

            if (isModalMode)
            {
                colorimeter.ShowCCDControlDialog();
            }
            else
            {
                if (colorimeter.CameraCtlDlg.IsVisible())
                {
                    colorimeter.CameraCtlDlg.Hide();
                    result = 1;
                }
                else
                {
                    colorimeter.CameraCtlDlg.Show();
                    result = 2;
                }
            }

            return result;
        }

        #region calibrate

        private double[] Mean(double[, ,] color)
        {
            double[] v = new double[3];
            int count = v.GetLength(0) * v.GetLength(1);

            for (int i = 0; i < v.GetLength(0); i++)
            {
                for (int j = 0; j < v.GetLength(1); j++)
                {
                    v[0] += color[i, j, 0];
                    v[1] += color[i, j, 1];
                    v[2] += color[i, j, 2];
                }
            }

            v[0] /= count;
            v[1] /= count;
            v[2] /= count;

            return v;
        }

        public void FocusCalibrate(Action callBack)
        {
            new Action(delegate()
            {
                IntegratingSphere intergrate = (IntegratingSphere)DevManage.Instance.SelectDevice(typeof(IntegratingSphere).Name);

                if (fixture != null)
                {
                    fixture.RotateOff();
                    fixture.IntegratingSphereUp();
                    intergrate.Lighten();
                    fixture.HoldIn();
                    fixture.BatteryOn();
                    while (!dut.CheckDUT()) { Thread.Sleep(100); }
                    Thread.Sleep(8000);
                    dut.ChangePanelColor(255, 255, 255);
                    colorimeter.StopVideo();
                }

                if (callBack != null)
                {
                    callBack();
                }
            }).BeginInvoke(null, null);
        }

        public void LvCalibrate(Action callBack, System.Windows.Forms.PictureBox cavaus)
        {
            float exposure = 4096;
            float runExp = exposure;
            float maxExp = exposure;
            float minExp = 1;

            new Action(delegate()
            {
                IntegratingSphere sphere = (IntegratingSphere)DevManage.Instance.SelectDevice(typeof(IntegratingSphere).Name);

                try
                {
                    if (ca310Hanle == null)
                    {
                        ca310Hanle = new KonicaCa310();
                        ca310Hanle.Initiaze();
                        if (ca310Pipe.ErrorMessage == "")
                        {
                            System.Windows.Forms.MessageBox.Show("Please switch Ca310 to initial mode.");
                            ca310Hanle.Zero();
                            System.Windows.Forms.MessageBox.Show("Please switch Ca310 to measure mode.");
                        }
                    }

                    fixture.HoldOut();
                    fixture.IntegratingSphereUp();
                    sphere.Lighten();
                    System.Windows.Forms.MessageBox.Show("Software will waitting 5 minutes");
                    System.Threading.Thread.Sleep(300000);
                    fixture.RotateOn();
                    // CIE1931Value value = ca310Pipe.GetCa310Data();
                    CIE1931Value value = ca310Hanle.GetCa310Data();

                    fixture.RotateOff();
                    fixture.CameraDown();
                    fixture.MotorMove(150000);
                    System.Threading.Thread.Sleep(26000);

                    foreach (TestItem item in Items)
                    {
                        if (item.RGB == Color.FromArgb(255, 255, 255))
                        {
                            do
                            {
                                colorimeter.ExposureTime = runExp;
                                System.Drawing.Bitmap bitmap = colorimeter.GrabImage();

                                if (cavaus != null)
                                {
                                    cavaus.Image = System.Drawing.Image.FromHbitmap(bitmap.GetHbitmap());
                                }

                                double[] rgbMean = this.Mean(ip.bmp2rgb(bitmap));

                                if (rgbMean[0] > 230 || rgbMean[1] > 230 || rgbMean[2] > 230)
                                {
                                    maxExp = runExp;
                                    runExp = (runExp + minExp) / 2;
                                }
                                else if (rgbMean[0] < 215 || rgbMean[1] < 215 || rgbMean[2] < 215)
                                {
                                    minExp = runExp;
                                    runExp = (runExp + maxExp) / 2;
                                }
                                else
                                {
                                    optimalExp = runExp;
                                    item.Exposure = optimalExp;
                                }

                                if (Math.Abs(minExp) == Math.Abs(maxExp))
                                {
                                    throw new Exception("Luminance calibration fail.");
                                }
                            }
                            while (true);
                        }
                        else
                        {
                            double faction = (redWeight * item.RGB.R + greenWeight * item.RGB.G + blueWeight * item.RGB.B) / 255;

                            if (faction == 0)
                            {
                                item.Exposure = 4000;
                            }

                            item.Exposure = (float)(optimalExp / faction);
                        }
                    }

                    if (callBack != null)
                    {
                        callBack();
                    }
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message);
                }
                finally
                {
                    if (sphere != null)
                    {
                        sphere.Lightoff();
                    }

                    if (fixture != null)
                    {
                        fixture.IntegratingSphereDown();
                        fixture.CameraUp();
                        fixture.Reset();
                    }
                    xml.SaveScript();
                }
            }).BeginInvoke(null, null);
        }

        public void ColorCalibrate(Action callBack, string serialNumber)
        {
            new Action(delegate()
            {
                ICalibration clrCalibration = new ColorCalibration(dut, ca310Hanle, fixture, colorimeter);
                clrCalibration.SerialNumber = serialNumber;
                clrCalibration.Calibration(optimalExp);

                if (callBack != null)
                {
                    callBack();
                }
            }).BeginInvoke(null, null);
        }

        public void FlexCalibrate(Action callBack, string serialNumber)
        {
            new Action(delegate()
            {
                ICalibration flexCalib = new FlexCalibration(dut, colorimeter, ip);
                flexCalib.SerialNumber = serialNumber;
                flexCalib.Calibration(optimalExp);

                if (callBack != null)
                {
                    callBack();
                }
            }).BeginInvoke(null, null);
        }

        #endregion
    }

    public enum TestMode
    {
        None,
        Manual,
        Automatic,
        Ca310,
    }
}
