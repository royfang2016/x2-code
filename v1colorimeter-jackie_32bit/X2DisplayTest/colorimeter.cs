﻿using System;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using FlyCapture2Managed;
using FlyCapture2Managed.Gui;
using System.Collections.Generic;
using Hnwlxy.HmzSysPlatform;

namespace X2DisplayTest
{
    public delegate void DataChangeDelegate(object sender);

    public class CameraConfigData
    {
        public float Exposure { get; set; }
        public float Brightness { get; set; }
        public float Gamma { get; set; }
        public float Gain { get; set; }
        public float Shutter { get; set; }
    }

    public class Colorimeter
    {
        //private event DataChangeDelegate dataChange;
        private CameraControlDialog m_camCtlDlg;
        private ManagedCameraBase m_camera = null;
        private ManagedImage m_rawImage;
        private ManagedImage m_processedImage;
        private bool m_flagVideo;
        private System.Windows.Forms.PictureBox m_videoCavaus;

        private static Dictionary<string, CameraConfigData> cameraKeyValueData;

        static Colorimeter()
        {
            string[] keys = new string[] { "White", "Black", "Red", "Green", "Blue" };
            cameraKeyValueData = new Dictionary<string, CameraConfigData>(5);

            HmzIniFile inifile = new HmzIniFile(@".\profile.ini");

            for (int i = 0; i < keys.Length; i++)
            {
                CameraConfigData cameraConfigData = new CameraConfigData();
                cameraConfigData.Brightness = (float)inifile.ReadDouble(keys[i], "brightness");
                cameraConfigData.Exposure = (float)inifile.ReadDouble(keys[i], "exposure");
                cameraConfigData.Gamma = (float)inifile.ReadDouble(keys[i], "gamma");
                cameraConfigData.Gain = (float)inifile.ReadDouble(keys[i], "gain");
                cameraConfigData.Shutter = (float)inifile.ReadDouble(keys[i], "shutter");
                cameraKeyValueData.Add(keys[i], cameraConfigData);
            }
        }

        static void PrintBuildInfo()
        {
            FC2Version version = ManagedUtilities.libraryVersion;

            StringBuilder newStr = new StringBuilder();
            newStr.AppendFormat(
                "FlyCapture2 library version: {0}.{1}.{2}.{3}\n",
                version.major, version.minor, version.type, version.build);

            Console.WriteLine(newStr);
        }

        static void PrintCameraInfo(CameraInfo camInfo)
        {
            StringBuilder newStr = new StringBuilder();
            newStr.Append("\n*** CAMERA INFORMATION ***\n");
            newStr.AppendFormat("Serial number - {0}\n", camInfo.serialNumber);
            newStr.AppendFormat("Camera model - {0}\n", camInfo.modelName);
            newStr.AppendFormat("Camera vendor - {0}\n", camInfo.vendorName);
            newStr.AppendFormat("Sensor - {0}\n", camInfo.sensorInfo);
            newStr.AppendFormat("Resolution - {0}\n", camInfo.sensorResolution);

            Console.WriteLine(newStr);
        }

        public CameraControlDialog CameraCtlDlg
        {
            get {
                return m_camCtlDlg;
            }
        }

        /// <summary>
        /// get the temperature of ccd
        /// </summary>
        public double Temperature 
        {
            get {
                if (m_camera.IsConnected()) {
                    return m_camera.GetProperty(PropertyType.Temperature).valueA / 10 - 273.15;
                }
                else {
                    return double.NaN;
                }
            }
        }

        private Size imageSize;
        /// <summary>
        /// image size
        /// </summary>
        public Size ImageSize
        {
            get {
                return imageSize;
            }
        }

        /// <summary>
        /// frame rate
        /// </summary>
        public float FrameRate
        {
            get {
                if (m_camera.IsConnected()) {
                    return m_camera.GetProperty(PropertyType.FrameRate).absValue;
                }
                else {
                    return 0;
                }
            }
        }

        private TimeStamp timeStamp;
        /// <summary>
        /// time stamp
        /// </summary>
        public TimeStamp TimeStamp
        {
            get {
                if (timeStamp == null)
                {
                    timeStamp = new TimeStamp();
                }
                return timeStamp;
            }
        }

        /// <summary>
        /// ccd exposure time
        /// </summary>
        public double ExposureTime
        {
            get {
                if (m_camera.IsConnected()) {
                    return m_camera.GetProperty(PropertyType.AutoExposure).valueA;
                } 
                else {
                    return 0;
                }
            }
            set {
                if (m_camera.IsConnected()) {
                    CameraProperty property = new CameraProperty(PropertyType.AutoExposure);
                    property.absControl = false;
                    property.onOff = true;
                    property.autoManualMode = false;
                    property.valueA = (uint)value;
                    m_camera.SetProperty(property);
                }
            }
        }

        public float Shutter
        {
            get {
                if (m_camera.IsConnected()) {
                    return m_camera.GetProperty(PropertyType.Shutter).valueA;
                }
                else {
                    return 0;
                }
            }
            set {
                if (m_camera.IsConnected()) {
                    CameraProperty property = new CameraProperty(PropertyType.Shutter);
                    property.absControl = false;
                    property.onOff = true;
                    property.autoManualMode = false;
                    property.valueA = (uint)value;
                    try {
                        m_camera.SetProperty(property);
                    }
                    catch {
                        System.Threading.Thread.Sleep(500);
                        m_camera.SetProperty(property);
                    }
                }
            }
        }

        public float Gain
        {
            get {
                if (m_camera.IsConnected()) {
                    return m_camera.GetProperty(PropertyType.Gain).valueA;
                }
                else
                {
                    return 0;
                }
            }
            set {
                if (m_camera.IsConnected())
                {
                    CameraProperty property = new CameraProperty(PropertyType.Gain);
                    property.absControl = false;
                    property.onOff = true;
                    property.autoManualMode = false;
                    property.valueA = (uint)value;
                    m_camera.SetProperty(property);
                }
            }
        }

        private void ConfigPropertyValue(PropertyType type, float value)
        {
            if (m_camera.IsConnected())
            {
                CameraProperty property = new CameraProperty(type);
                property.absControl = false;
                property.onOff = true;
                property.autoManualMode = false;
                property.valueA = (uint)value;
                m_camera.SetProperty(property);
            }
        }

        public void ConfigCamera(Color color)
        {
            CameraConfigData cameraData = cameraKeyValueData[color.Name];

            if (cameraData != null)
            {
                ConfigPropertyValue(PropertyType.Brightness, cameraData.Brightness);
                ConfigPropertyValue(PropertyType.AutoExposure, cameraData.Exposure);
                ConfigPropertyValue(PropertyType.Gamma, cameraData.Gamma);
                ConfigPropertyValue(PropertyType.Gain, cameraData.Gain);
            }
        }

        public void RefushShutter(Color color)
        {
            CameraConfigData cameraData = cameraKeyValueData[color.Name];

            if (cameraData != null)
            {
                this.Shutter = cameraData.Shutter;
            }
        }

        public Colorimeter()
        {
            imageSize = new Size(0, 0);
            m_rawImage = new ManagedImage();
            m_processedImage = new ManagedImage();
            m_camCtlDlg = new CameraControlDialog();
        }

        public void ShowCCDControlDialog()
        {
            m_camCtlDlg.SetTitle("Colorimeter parameters  -- v1.0 by Microtest\n");
            m_camCtlDlg.ShowModal();
        }

        public bool Connect()
        {
            bool flag = false;
            CameraSelectionDialog camSlnDlg = new CameraSelectionDialog();

            camSlnDlg.Show();
            camSlnDlg.Hide();

            //if (camSlnDlg.ShowModal())
            {
                try {
                    ManagedPGRGuid[] selectedGuids = camSlnDlg.GetSelectedCameraGuids();
                    ManagedPGRGuid guidToUse = selectedGuids[0];

                    ManagedBusManager busMgr = new ManagedBusManager();
                    m_camera = new ManagedCamera();

                    // Connect to the first selected GUID
                    m_camera.Connect(guidToUse);
                    m_camCtlDlg.Connect(m_camera);

                    CameraInfo camInfo = m_camera.GetCameraInfo();
                    camInfo.vendorName = "MicroTest";
                    camInfo.modelName = "v1";
                   // UpdateFormCaption(camInfo);

                    // Set embedded timestamp to on
                    EmbeddedImageInfo embeddedInfo = m_camera.GetEmbeddedImageInfo();
                    embeddedInfo.timestamp.onOff = true;
                    //embeddedInfo.exposure.onOff = true;
                    embeddedInfo.shutter.onOff = true;
                    //tbox_uptime.Text = embeddedInfo.timestamp.ToString();
                    m_camera.SetEmbeddedImageInfo(embeddedInfo);
                    flag = true;
                }
                catch (IndexOutOfRangeException e) {
                    m_camCtlDlg.Disconnect();

                    if (m_camera != null)
                    {
                        m_camera.Disconnect(); 
                    }
                    flag = false;
                    throw e;
                }
            }

            return flag;
        }

        public void Disconnect()
        {
            m_camCtlDlg.Hide();
            m_camCtlDlg.Disconnect();
            m_camera.Disconnect();
        }
        
        public Bitmap GrabImage()
        {
            try
            {
                if (m_camera.IsConnected())
                {
                    m_camera.StartCapture();
                    m_camera.RetrieveBuffer(m_rawImage);
                    m_rawImage.Convert(PixelFormat.PixelFormatBgr, m_processedImage);
                    m_camera.StopCapture();
                    imageSize.Width = (int)m_rawImage.cols;
                    imageSize.Height = (int)m_rawImage.rows;
                    timeStamp = m_rawImage.timeStamp;
                }
            }
            catch (FC2Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }

            return m_processedImage.bitmap;
        }

        public void SetVideoCavaus(System.Windows.Forms.PictureBox cavaus)
        {
            m_videoCavaus = cavaus;
        }

        public void PlayVideo()
        {
            m_flagVideo = true;

            try {
                if (m_camera.IsConnected()) {                    
                    new Action(delegate(){
                        while (m_flagVideo) {
                            if (m_videoCavaus != null)
                            {
                                m_videoCavaus.Image = this.GrabImage();
                            }
                            System.Threading.Thread.Sleep(35);
                        }
                    }).BeginInvoke(null, null);
                }
            }
            catch {
            }
        }

        public void StopVideo()
        {
            m_flagVideo = false;
        }

        public void RunSingleCamera(ManagedPGRGuid guid)
        {
            const int k_numImages = 10;

            ManagedCamera cam = new ManagedCamera();

            // Connect to a camera
            cam.Connect(guid);

            // Get the camera information
            CameraInfo camInfo = cam.GetCameraInfo();

            PrintCameraInfo(camInfo);

            // Get embedded image info from camera
            EmbeddedImageInfo embeddedInfo = cam.GetEmbeddedImageInfo();

            // Enable timestamp collection	
            if (embeddedInfo.timestamp.available == true)
            {
                embeddedInfo.timestamp.onOff = true;
            }

            // Set embedded image info to camera
            cam.SetEmbeddedImageInfo(embeddedInfo);

            // Start capturing images
            cam.StartCapture();

            // Create a raw image
            ManagedImage rawImage = new ManagedImage();

            // Create a converted image
            ManagedImage convertedImage = new ManagedImage();

            for (int imageCnt = 0; imageCnt < k_numImages; imageCnt++)
            {
                // Retrieve an image
                cam.RetrieveBuffer(rawImage);

                // Get the timestamp
                TimeStamp timeStamp = rawImage.timeStamp;

                Console.WriteLine(
                   "Grabbed image {0} - {1} {2} {3}",
                   imageCnt,
                   timeStamp.cycleSeconds,
                   timeStamp.cycleCount,
               timeStamp.cycleOffset);

                // Convert the raw image
                rawImage.Convert(PixelFormat.PixelFormatBgr, convertedImage);

                // Create a unique filename
                string filename = String.Format(
                   "FlyCapture2Test_CSharp-{0}-{1}.bmp",
                   camInfo.serialNumber,
                   imageCnt);

                // Get the Bitmap object. Bitmaps are only valid if the
                // pixel format of the ManagedImage is RGB or RGBU.
                System.Drawing.Bitmap bitmap = convertedImage.bitmap;

                // Save the image
                bitmap.Save(filename);
            }

            // Stop capturing images
            cam.StopCapture();

            // Disconnect the camera
            cam.Disconnect();
        }
    }
}
