//=============================================================================
// Main UI function. Start from Program.cs and enter Form1_load 
//=============================================================================
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace X2DisplayTest
{
    public partial class Form_Config : Form
    {
        private TabPage m_preTabPage;
        private Config config;
        private Engine engine;
        private System.Drawing.Image preImage;
        private PointF m_ptStart, m_ptStop;

        private string serialNumber;
        private bool isdemomode = false; //Demo mode can only be used for analysis tab.

        private string productType = @"Hodor";
        public Form_Config()
        {
           // ReadCurrentProDuctRecord();
            InitializeComponent();
            Form.CheckForIllegalCrossThreadCalls = false;
            picturebox_config.SizeMode = PictureBoxSizeMode.StretchImage;
            serialNumber = "";
        }

        public void ReadCurrentProDuctRecord()
        {
            string FileName = @".\productType.txt";
            if (!File.Exists(FileName))
            {
                FileStream stream = File.Create(FileName);
                stream.Close();

                using (StreamWriter sw = new StreamWriter(FileName, false))
                {
                    sw.WriteLine("Hodor");
                    sw.Flush();
                    sw.Close();
                }
             
            }


            using (StreamReader sw = new StreamReader(FileName))
            {
               productType = sw.ReadToEnd().ToString().Trim();
                sw.Close();
            }
            if (productType.ToString().Trim() == "")
            {
                using (StreamWriter sw = new StreamWriter(FileName, false))
                {
                    sw.WriteLine("Hodor");
                    sw.Flush();
                    sw.Close();
                }
                productType = "Hodor";
            }


        }
        // UI related
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Hide();
            m_preTabPage = Tabs.TabPages[0];
            ReadCurrentProDuctRecord();
            try 
            {
                string frofileName = @".\profile.ini";
                if (productType == "Bran")
                {
                    frofileName = @".\profile-bran.ini";
                }
                else
                {
                    frofileName = @".\profile-hodor.ini";
                }

                config = new Config(frofileName);

                ModeSwithDlg dlg = new ModeSwithDlg();
                dlg.ShowDialog();
                isdemomode = dlg.IsAnalysisPanel;

                if (!isdemomode) {
                    engine = new Engine(config);
                    engine.Initilazie();
                    engine.VideoCavus(picturebox_test);
                    engine.dataChange += new DataDelegate(engine_dataChange);
                    engine.tableDataChange += new TableViewDelegate(engine_tableDataChange);
  
                    FlushDataGridView(engine.Items);
                    UpdateConfigVlaue();

                    if (engine.TestMode == TestMode.Automatic) {
                        rbtn_auto.Checked = true;
                        dgvCa310Data.Visible = false;
                        sslMode.Text = "Automatic mode";
                    }
                    else if (engine.TestMode == TestMode.Ca310) {
                        rbtn_Ca310.Checked = true;
                        dgvCa310Data.Visible = true;
                        sslMode.Text = "Ca-310 mode";
                    }
                    else if (engine.TestMode == TestMode.Manual) {
                        rbtn_manual.Checked = true;
                        dgvCa310Data.Visible = false;
                        sslMode.Text = "Manual mode";
                    }
                    tbSerialNumber.ReadOnly = config.IsScanSerialNumber;
                }
                else {
                    Tabs.SelectedTab = tab_Analysis;
                }                
            }
            catch (IndexOutOfRangeException) {
                MessageBox.Show("Cann't find colorimeter.");
                Application.Exit();
            }
            catch (NullReferenceException ex) {
                MessageBox.Show(ex.Message);
                Application.Exit();
            }
            catch {
                MessageBox.Show("Unexpect exception.");
            }

            this.Show();
            tbox_sn.Focus();
        }

        private void UpdateConfigVlaue()
        {
            if (config.IsOnlineShopfloor) {
                tbSFCMode.Text = "Online";
                tbSFCMode.ForeColor = Color.White;
                tbSFCMode.BackColor = Color.Green;
            }
            else {
                tbSFCMode.Text = "Offline";
                tbSFCMode.ForeColor = Color.DarkRed;
                tbSFCMode.BackColor = SystemColors.Info;
            }

            tsslProduct.Text = "Product: " + engine.Dut.ToString();
            tbox_sn.Enabled = config.IsScanSerialNumber;

            if (config.IsScanSerialNumber) {
                this.tbox_sn.TextChanged += new System.EventHandler(this.tbox_sn_TextChanged);
            }
            else {
                this.tbox_sn.TextChanged -= new System.EventHandler(this.tbox_sn_TextChanged);
            }
        }

        private void  engine_tableDataChange(object sender, TableViewEventArgs args)
        {
            if (args.CurrentDevice == "Camera") {
                dgvData.Rows[args.Index].SetValues(args.Items[args.Index].TestName, 
                    args.Items[args.Index].TestNodes[3].Value, args.Items[args.Index].TestNodes[4].Value,
                    args.Items[args.Index].TestNodes[5].Value, args.Items[args.Index].TestNodes[0].Value, 
                    args.Items[args.Index].TestNodes[1].Value, args.Items[args.Index].TestNodes[2].Value);
                double result = args.Items[args.Index].TestNodes[1].Value;
                bool bresult = args.Items[args.Index].TestNodes[1].Value <= args.Items[args.Index].TestNodes[1].Upper &&
                    args.Items[args.Index].TestNodes[1].Value >= args.Items[args.Index].TestNodes[1].Lower;
                if (!bresult && result > 0)
                {
                    dgvData[5, args.Index].Style.BackColor = Color.Red;
                }
                else
                {
                    dgvData[5, args.Index].Style.BackColor = Color.FloralWhite;
                }

            }
            else if (args.CurrentDevice == "Ca310") {
                dgvCa310Data.Rows[args.Index].SetValues(args.Items[args.Index].TestName,
                        args.Items[args.Index].TestNodes[3].Value, args.Items[args.Index].TestNodes[4].Value, 
                        args.Items[args.Index].TestNodes[5].Value);
                for (int i = 3; i <= 5; i++)
                {
                    if (args.Items[args.Index].TestNodes[i].Result == false && args.Items[args.Index].TestNodes[i].Value > 0)
                    {
                        dgvCa310Data[i-2,args.Index].Style.BackColor = Color.Red;
                    }
                    else
                    {
                        dgvCa310Data[i-2,args.Index].Style.BackColor = Color.FloralWhite;
                    }

                }
                
            }
        }

        private void engine_dataChange(object sender, DataChangeEventArgs args)
        {
            sslStatus.Text = args.StatusInfo;
            tbox_ccdtemp.Text = args.CCDTemperature.ToString();
            tbox_uptime.Text = String.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                args.Uptime.Hours, args.Uptime.Minutes, args.Uptime.Seconds);
            tbox_colorimeterstatus.Text = args.CCDStatusInfo;

            if (args.Image != null && args.Image != preImage) {
                picturebox_test.Image = args.Image;
                preImage = args.Image;
            }

            if (!config.IsScanSerialNumber) {
                tbox_sn.Text = args.SerialNumber;
            }

            if (engine.IsDutReady) {
                tbox_dut_connect.Text = "Connected";
                tbox_dut_connect.ForeColor = Color.White;
                tbox_dut_connect.BackColor = Color.Green;
            }
            else {
                tbox_dut_connect.Text = "TBD";
                tbox_dut_connect.ForeColor = Color.Black;
                tbox_dut_connect.BackColor = Color.FromArgb(244, 244, 244);
            }

            if (engine.IsShopFlowReady) {
                tbox_shopfloor.Text = "Connected";
                tbox_shopfloor.ForeColor = Color.White;
                tbox_shopfloor.BackColor = Color.Green;
            }
            else {
                tbox_shopfloor.Text = "TBD";
                tbox_shopfloor.ForeColor = Color.Black;
                tbox_shopfloor.BackColor = Color.FromArgb(244, 244, 244);
            }

            tbox_pf.Text = args.PassOrFail;

            if (args.PassOrFail == "READY" || args.PassOrFail == "TESTING") {
                tbox_pf.ForeColor = Color.Blue;
                tbox_pf.BackColor = SystemColors.Info;
            }
            else if (args.PassOrFail == "PASS") {
                tbox_pf.ForeColor = Color.White;
                tbox_pf.BackColor = Color.Green;
            }
            else if (args.PassOrFail == "FAIL") {
                tbox_pf.ForeColor = Color.White;
                tbox_pf.BackColor = Color.Red;
            }

        }

        private void FlushDataGridView(List<TestItem> items)
        {
    
            dgvData.Rows.Clear();
            dgvCa310Data.Rows.Clear();

            dgvData.Rows.Add(items.Count);
            dgvCa310Data.Rows.Add(items.Count);

            for (int i = 0; i < items.Count; i++)
            {
                dgvData.Rows[i].SetValues(items[i].TestName, items[i].TestNodes[0].Value, items[i].TestNodes[1].Value, 
                    items[i].TestNodes[2].Value, items[i].TestNodes[3].Value, items[i].TestNodes[4].Value, items[i].TestNodes[5].Value);

                if (engine.TestMode == TestMode.Ca310)
                {
                    dgvCa310Data.Rows[i].SetValues(items[i].TestName, 
                        items[i].TestNodes[3].Value, items[i].TestNodes[4].Value, items[i].TestNodes[5].Value);
                }
            }
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            if (btn_start.Text == "Start") {
                engine.Start();
                btn_start.Text = "Stop";
            }
            else if (btn_start.Text == "Stop") {
                engine.Stop();
                btn_start.Text = "Start";
            }
        }

        // mode choice
        private void TestMode_Changed(object sender, EventArgs e)
        {
            //fixture.Reset();
            engine.Stop();
            btn_start.Text = "Start";

            RadioButton mode = sender as RadioButton;

            if (mode.Checked && mode.Text == "Manual")
            {
                sslMode.Text = "Manual mode";
                engine.TestMode = TestMode.Manual;
                dgvCa310Data.Visible = false;
            }
            else if (mode.Checked && mode.Text == "Automatic")
            {
                sslMode.Text = "Automatic mode";
                engine.TestMode = TestMode.Automatic;
                dgvCa310Data.Visible = false;
            }
            else if (mode.Checked && mode.Text == "Ca310")
            {
                sslMode.Text = "Ca-310 mode";
                engine.TestMode = TestMode.Ca310;
                dgvCa310Data.Visible = true;
                FlushDataGridView(engine.Items);
            }
        }

        private void tsbtnSetting_Click(object sender, EventArgs e)
        {
            FrmLogin login = new FrmLogin();

            if (DialogResult.OK == login.ShowDialog())
            {
                login.Close();
                FrmSetting setDlg = new FrmSetting(config, engine.Items);
                setDlg.ShowDialog();
                engine.Dut = setDlg.ActiveDUT;
                UpdateConfigVlaue();
            }
        }

        // tab page select
        private void Tabs_Selecting(object sender, TabControlCancelEventArgs e)
        {
            TabControl tabControl = sender as TabControl;

            if ((tabControl.SelectedTab != m_preTabPage))
            {
                if (tabControl.SelectedTab == tabControl.TabPages[0]) {
                    m_preTabPage = tabControl.SelectedTab;
                }
                else {
                    engine.Stop();
                    rbtn_manual.Checked = true;

                    FrmLogin login = new FrmLogin();

                    if (DialogResult.OK == login.ShowDialog()) {
                        m_preTabPage = tabControl.SelectedTab;
                        Btn_Lv.Enabled = Btn_Size.Enabled = Btn_Color.Enabled = Btn_FF.Enabled = false;
                        btn_focus.Enabled = true;
                    }
                    else {
                        tabControl.SelectedTab = m_preTabPage;
                    }
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                engine.Exit();
                toolStripButtonStop.PerformClick();       
            }
            catch (NullReferenceException)
            {
                // Nothing to do here
            }
        }

        private void tbox_sn_TextChanged(object sender, EventArgs e)
        {
            if (tbox_sn.Text.Length == engine.SNLength)      //fake condition. More input is needed from Square
            {
                tbox_sn.SelectAll();
                tbox_sn.Focus();
                serialNumber = tbox_sn.Text;
                engine.SerialNumber = tbox_sn.Text;

            }
        }

        private void tbSerialNumber_TextChanged(object sender, EventArgs e)
        {
            serialNumber = "";

            if (tbSerialNumber.Text.Length == engine.SNLength) //fake condition. More input is needed from Square
            {
                tbSerialNumber.SelectAll();
                tbSerialNumber.Focus();
                serialNumber = tbSerialNumber.Text;

            }
        }

        #region calibration event

        private System.Drawing.PointF ptFirstLine;
        private bool isFirstLine = true;

        private void Tabs_Selected(object sender, TabControlEventArgs e)
        {
            TabControl page = (TabControl)sender;

            if (page.SelectedTab == Tab_Config) {
                Btn_Size.Enabled = Btn_Lv.Enabled = Btn_Color.Enabled = Btn_FF.Enabled = false;
                lbMM.Visible = lbTips.Visible = tbSizeCal.Visible = false;
                engine.Video(picturebox_config, true);
            }
            else {
                engine.Video(picturebox_config, false);
            }
        }

        private void btn_focus_Click(object sender, EventArgs e)
        {
            lbCalibration.Text = "Focus calibration...";
            engine.FocusCalibrate(new Action(delegate() {
                MessageBox.Show("Adjust Colorimeter Focus");
                Btn_Size.Enabled = true;
                btn_focus.Enabled = false;
            }));
        }

        private void Btn_Size_Click(object sender, EventArgs e)
        {
            int x = (picturebox_config.Location.X + picturebox_config.Width) / 2;
            int y = (picturebox_config.Location.Y + picturebox_config.Height) / 2;
            Cursor.Position = this.PointToScreen(new System.Drawing.Point(x, y));
            this.picturebox_config.MouseClick += new System.Windows.Forms.MouseEventHandler(this.picturebox_config_MouseClick);
        }

        private void picturebox_config_MouseClick(object sender, MouseEventArgs e)
        {
            if (isFirstLine) {
                this.picturebox_config.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picturebox_config_MouseMove);
            }
            else {
                this.picturebox_config.MouseMove -= new System.Windows.Forms.MouseEventHandler(this.picturebox_config_MouseMove);
            }

            if (picturebox_config.Image != null) {
                Pen pen = new Pen(Color.Red, 3);
                Graphics g = Graphics.FromImage(picturebox_config.Image);
                System.Drawing.PointF pt = picturebox_config.PointToClient(Cursor.Position);
                ptFirstLine = new PointF(2016.0f / picturebox_config.Width * pt.X, 2016.0f / picturebox_config.Height * pt.Y);
                g.DrawLine(pen, new PointF(ptFirstLine.X, 0), new PointF(ptFirstLine.X, 2016));
                picturebox_config.Invalidate();

                if (isFirstLine) {
                    m_ptStart = ptFirstLine;
                }
                else {
                    m_ptStop = ptFirstLine;
                }
            }
            isFirstLine = !isFirstLine;

            // 3条线已画完
            if (isFirstLine) {
                this.picturebox_config.MouseClick -= new System.Windows.Forms.MouseEventHandler(this.picturebox_config_MouseClick);
                lbMM.Visible = lbTips.Visible = tbSizeCal.Visible = true;
                tbSizeCal.Focus();
            }
        }

        private void picturebox_config_MouseMove(object sender, MouseEventArgs e)
        {
            if (picturebox_config.Image != null) {
                Pen pen = new Pen(Color.Red, 3);
                Graphics g = Graphics.FromImage(picturebox_config.Image);
                System.Drawing.PointF pt = picturebox_config.PointToClient(Cursor.Position);
                pt = new PointF(2016.0f / picturebox_config.Width * pt.X, 2016.0f / picturebox_config.Height * pt.Y);
                g.DrawLine(pen, new PointF(ptFirstLine.X, ptFirstLine.Y), new PointF(pt.X, ptFirstLine.Y));
                picturebox_config.Invalidate();
            }
        }

        private void picturebox_config_MouseHover(object sender, EventArgs e)
        {
            System.Drawing.Point ptCursor = picturebox_config.PointToClient(Cursor.Position);

            if (ptCursor.X <= 0)
            {
                ptCursor.X = 0;
            }
            if (ptCursor.X >= picturebox_config.Width)
            {
                ptCursor.X = picturebox_config.Width;
            }
            if (ptCursor.Y <= 0)
            {
                ptCursor.Y = 0;
            }
            if (ptCursor.Y >= picturebox_config.Height)
            {
                ptCursor.Y = picturebox_config.Height;
            }
        }

        private void tbSizeCal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                double value;

                if (!double.TryParse(tbSizeCal.Text.TrimEnd('\n'), out value))
                {
                    MessageBox.Show("Please type a number.");
                    tbSizeCal.Text = "";
                    return;
                }
                lbMM.Visible = lbTips.Visible = tbSizeCal.Visible = false;
                btn_focus.Enabled = Btn_Size.Enabled = Btn_Color.Enabled = Btn_FF.Enabled = false;
                Btn_Lv.Enabled = true;
                float pixel = m_ptStop.X - m_ptStart.X;
                engine.XmlManage.PixelDistRatio = (pixel / value);
            }
        }

        private void Btn_Lv_Click(object sender, EventArgs e)
        {
            lbCalibration.Text = "Luminance calibration...";
            btn_focus.Enabled = Btn_Size.Enabled = Btn_Lv.Enabled = Btn_FF.Enabled = Btn_Color.Enabled = false;

            engine.LvCalibrate(new Action(delegate() {
                    btn_focus.Enabled = Btn_Size.Enabled = Btn_Lv.Enabled = Btn_FF.Enabled = false;
                    Btn_Color.Enabled = true;
                    lbCalibration.Text = "Luminance calibration finish.";
                }), picturebox_config);
        }

        private void Btn_Color_Click(object sender, EventArgs e)
        {
            lbCalibration.Text = "Color calibration...";
            btn_focus.Enabled = Btn_Size.Enabled = Btn_Lv.Enabled = Btn_Color.Enabled = Btn_FF.Enabled = false;

            engine.ColorCalibrate(new Action(delegate() {
                    btn_focus.Enabled = Btn_Size.Enabled = Btn_Lv.Enabled = Btn_Color.Enabled = false;
                    Btn_FF.Enabled = true;
                    lbCalibration.Text = "Color calibration finish.";
                }), serialNumber);
        }

        private void Btn_FF_Click(object sender, EventArgs e)
        {
            lbCalibration.Text = "Flex calibration...";
            Btn_FF.Enabled = Btn_Size.Enabled = Btn_Lv.Enabled = Btn_Color.Enabled = btn_focus.Enabled = false;
            engine.FlexCalibrate(new Action(delegate() {
                    Btn_Color.Enabled = Btn_Size.Enabled = Btn_Lv.Enabled = Btn_FF.Enabled = false;
                    btn_focus.Enabled = true;
                    lbCalibration.Text = "Flex calibration finish.";
                }), serialNumber);
        }

        #endregion

        #region events
        private void OnNewCameraClick(object sender, EventArgs e)
        {
            toolStripButtonStart.PerformClick();
            engine.Stop();
            engine = null;
            config = null;
            Form1_Load(sender, e);
        }

        private void toolStripButtonStart_Click(object sender, EventArgs e)
        {
            engine.Video(picturebox_test, true);
            toolStripButtonStart.Enabled = false;
            toolStripButtonStop.Enabled = true;
        }

        private void toolStripButtonStop_Click(object sender, EventArgs e)
        {
            engine.Video(picturebox_test, false);
            toolStripButtonStart.Enabled = true;
            toolStripButtonStop.Enabled = false;
        }

        private void toolStripButtonCameraControl_Click(object sender, EventArgs e)
        {
            int result = engine.ShowColorimeterDialog(true);

            if (result == 1) {
                tsbCameraControl.Enabled = true;
            }
            else if ( result == 2) {
                tsbCameraControl.Enabled = false;
            }
        }

        private void realSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            realSizeToolStripMenuItem.CheckState = CheckState.Checked;
            stretchToFillToolStripMenuItem.CheckState = CheckState.Unchecked;
            picturebox_test.SizeMode = PictureBoxSizeMode.Normal;
            picturebox_test.Refresh();
        }

        private void stretchToFillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            realSizeToolStripMenuItem.CheckState = CheckState.Unchecked;
            stretchToFillToolStripMenuItem.CheckState = CheckState.Checked;
            picturebox_test.SizeMode = PictureBoxSizeMode.StretchImage;
            picturebox_test.Refresh();
        }
        #endregion


        //private void UpdateFormCaption(CameraInfo camInfo)
        //{
        //    String captionString = String.Format(
        //        "X2 Display Test Station - {0} {1} ({2})",
        //        camInfo.vendorName,
        //        camInfo.modelName,
        //        camInfo.serialNumber);
        //    this.Text = captionString;
        //}

        private void StartGrabLoop()
        {
            //m_grabThread = new BackgroundWorker();
            //m_grabThread.ProgressChanged += new ProgressChangedEventHandler(UpdateUI);
            //m_grabThread.DoWork += new DoWorkEventHandler(GrabLoop);
            //m_grabThread.WorkerReportsProgress = true;
            //m_grabThread.RunWorkerAsync();
        }

         // analysis related
        private void btn_openrawfile_Click(object sender, EventArgs e)
        {
            try
            {
                if (rbtn_colorimeter.Checked)
                {
                    //m_rawImage.Convert(FlyCapture2Managed.PixelFormat.PixelFormatBgr, m_processedImage);
                    //picturebox_raw.Image = m_processedImage.bitmap;
                }
                else if (rbtn_loadfile.Checked)
                {

                    OpenFileDialog ofd = new OpenFileDialog();
                    ofd.Title = "Open Image";
                    ofd.Filter = "bmp files (*.bmp) | *.bmp";
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        picturebox_raw.Refresh();
                        picturebox_raw.Image = (Bitmap)System.Drawing.Image.FromFile(ofd.FileName);

                    }
                    else
                    {
                        MessageBox.Show("Please check Image Source");
                    }
                    ofd.Dispose();
                }
                Show();
            }
            catch
            {
                //Debug.WriteLine("Error: " + ex.Message);
                return;
            }
        }

        private void btn_process_Click(object sender, EventArgs e)
        {
            try
            {

                if (rbtn_corner.Checked)
                {

                }
                else if (rbtn_9ptuniformity.Checked)
                {
                }
                else if (rbtn_16ptuniformity.Checked)
                {
                }
                else if (rbtn_worstzone.Checked)
                {
                }
                else if (rbtn_cropping.Checked)
                {
                    //Bitmap rawimg = new Bitmap(picturebox_raw.Image);
                    ////Process Image to 1bpp to increase SNR 
                    //Bitmap m_orig = rawimg.Clone(new Rectangle(0, 0, rawimg.Width, rawimg.Height), System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                    //// only support the 32bppArgb for Aforge Blob Counter
                    //Bitmap processbmp = m_orig.Clone(new Rectangle(0, 0, m_orig.Width, m_orig.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    //ip.GetDisplayCornerfrombmp(processbmp, out displaycornerPoints);
                    //Bitmap desimage = croppingimage(rawimg, displaycornerPoints);
                    //refreshtestimage(desimage, pictureBox_processed);
                    //Bitmap cropimage = ip.croppedimage(rawimg, displaycornerPoints, dut.ui_width, dut.ui_height);
                    //refreshtestimage(cropimage, pictureBox_processed);
                }
                else if (rbtn_5zone.Checked)
                {
                    // load cropped bin image
                }
                    
                else
                {
                    MessageBox.Show("Please check processing item");
                }
            }
            catch
            {
                //Debug.WriteLine("Error: " + ex.Message);
                return;
            }
        }
    }   
}


