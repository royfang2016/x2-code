//=============================================================================
// Imaging Pipeline for display test. after zone parsing and resizing, the next 
// step is to process the XYZ data to meaningful display test metric. If possible
// there might be need to convert to other color space like La*b* 
//=============================================================================




using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using AForge;
using AForge.Imaging;
using AForge.Math;
using AForge.Math.Geometry;
using FlyCapture2Managed;

namespace X2DisplayTest
{
    public class imagingpipeline
    {
        private List<IntPoint> flagPoints;
        private Testlog log;
        private double[, ,] rgbstr;

        public List<double> lvvalue;
        public List<double> lvValue
        {
            get
            {
                return lvvalue;
            }
        }

        public double[, ,] RGB
        {
            get {
                return rgbstr;
            }
        }

        public Bitmap croppedimage(Bitmap src, List<IntPoint> displaycornerPoints, int width, int height)
        {
            //Create crop filter
            AForge.Imaging.Filters.SimpleQuadrilateralTransformation filter
                = new AForge.Imaging.Filters.SimpleQuadrilateralTransformation(displaycornerPoints, width, height);
            //Create cropped display image
            Bitmap des = filter.Apply(src);
            
            return des;
        }

        public void GetDisplayCornerfrombmp(Bitmap processbmp, out List<IntPoint> displaycornerPoints)
        {
            BlobCounter bbc = new BlobCounter();
            bbc.FilterBlobs = true;
            bbc.MinHeight = 5;
            bbc.MinWidth = 5;

            bbc.ProcessImage(processbmp);

            Blob[] blobs = bbc.GetObjectsInformation();
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

            foreach (var blob in blobs)
            {
                List<IntPoint> edgePoints = bbc.GetBlobsEdgePoints(blob);
                List<IntPoint> cornerPoints;


                // use the shape checker to extract the corner points
                if (shapeChecker.IsQuadrilateral(edgePoints, out cornerPoints))
                {
                    // only do things if the corners from a rectangle 
                    if (shapeChecker.CheckPolygonSubType(cornerPoints) == PolygonSubType.Rectangle)
                    {
                        flagPoints = cornerPoints;
                        List<IntPoint> tmpPoints = new List<IntPoint>(flagPoints);
                        int max = new int[]{tmpPoints[0].X, tmpPoints[1].X, tmpPoints[2].X, tmpPoints[3].X}.Max();
                        int min = new int[] { tmpPoints[0].X, tmpPoints[1].X, tmpPoints[2].X, tmpPoints[3].X }.Min();

                        if (max - min > 500)
                        {
                            break;
                        }

                        continue;
                        //break;
                    }
                    else
                    {                        
                        flagPoints = null;
                        continue;
                    }
                }

            }

            if (flagPoints == null)
                MessageBox.Show("Cannot Find the Display");

            displaycornerPoints = flagPoints;

        }

        public void GetDisplayCorner(ManagedImage m_processedImage, out List<IntPoint> displaycornerPoints)
        {
            // get display corner position 
            //Process Image to 1bpp to increase SNR 
            Bitmap m_orig = m_processedImage.bitmap.Clone(new Rectangle(0, 0, m_processedImage.bitmap.Width, m_processedImage.bitmap.Height), System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
            // only support the 32bppArgb for Aforge Blob Counter
            Bitmap processbmp = m_orig.Clone(new Rectangle(0, 0, m_orig.Width, m_orig.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            BlobCounter bbc = new BlobCounter();
            bbc.FilterBlobs = true;
            bbc.MinHeight = 5;
            bbc.MinWidth = 5;

            bbc.ProcessImage(processbmp);

            Blob[] blobs = bbc.GetObjectsInformation();
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

            foreach (var blob in blobs)
            {
                List<IntPoint> edgePoints = bbc.GetBlobsEdgePoints(blob);
                List<IntPoint> cornerPoints;
                int count = 0;

                // use the shape checker to extract the corner points
                if (shapeChecker.IsQuadrilateral(edgePoints, out cornerPoints))
                {
                    // only do things if the corners from a rectangle 
                    if (shapeChecker.CheckPolygonSubType(cornerPoints) == PolygonSubType.Rectangle)
                    {
                        flagPoints = cornerPoints;
                        continue;
                    }
                    else
                    {
                        MessageBox.Show("Cannot Find the Display");
                        flagPoints = null;
                        //                       picturebox_test.Image = m;
                        count++;
                        if (count < 3)
                        {
                            continue;
                        }
                        else
                        {
                            MessageBox.Show("Cannot Find the Display for 3 times. Quit Program");
                            Environment.ExitCode = -1;
                            Application.Exit();


                        }
                    }
                }

            }
            displaycornerPoints = flagPoints;

        }

        // get X/Y/Z
        private double getweight(double[, ,] XYZ, int index)
        {
            if (index < 0) {
                index = 0;
            }
            else if (index > 2) {
                index = 2;
            }

            double sum = 0;
            double mean = 0;
            float w = XYZ.GetLength(0);
            float h = XYZ.GetLength(1);

            for (int r = 0; r < w; r++)
            {
                for (int c = 0; c < h; c++)
                {
                    sum += XYZ[r, c, index];
                }

            }
            mean = sum / (w * h);
            return mean;
        }

        // get xyY, return:x = xyY[0], y = xyY[1], Y = luminance
        // 5 decimal places
        public double[] getxyY(double[, ,] XYZ)
        {
            double[] xyY = new double[3];
            double X = getweight(XYZ, 0);
            double Y = getweight(XYZ, 1);
            double Z = getweight(XYZ, 2);

            double sum = X + Y + Z;
            xyY[0] = Math.Round(X / sum, 5);
            xyY[1] = Math.Round(Y / sum, 5);
            xyY[2] = Y;

            return xyY;
        }

        // get average Y from input XYZ matrix
        public double getlv(double[, ,] XYZ)
        {
            double sum = 0;
            double mean = 0;
            float w = XYZ.GetLength(0);
            float h = XYZ.GetLength(1);
            List<double> pointLv = new List<double>();
            double value = 0;

            for (int r = 0; r < w; r++)
            {
                for (int c = 0; c < h; c++)
                {
                    value = XYZ[r, c, 1];
                    sum += XYZ[r, c, 1];
                    pointLv.Add(value);
                }

            }


            pointLv.Sort();
            int index = (int)(w*h*3/10.0);
          
            pointLv.RemoveRange(0, index);
            pointLv.RemoveRange(pointLv.Count-index, index);

            double sumAverage = pointLv.Average();
            return sumAverage;
           
            
           // mean = sum / (w * h);
           //return mean;
        }

        public List<double> PointLV(List<double> point)
        {
            List<double> points = point;
       
            point.Sort();
            int count = point.Count;
            double max = point.Max();
            double min = point.Min();

            int midle = count / 2;

            double max2 = point[count - 2];

            double min2 = point[1];
            double min3 = point[2];

            for (int i = 0; i < count; i++)
            {
                if (point[i] < 3)
                {
                    point[i] /= 5.5;
                }
                if (max == points[i])
                {
                    points[i] = (max+point[midle])/2;
                }
                if (max2 == points[i])
                {
                    points[i] = (max2 + point[midle]) / 2;
                }
                if (min == points[i])
                {
                    points[i] = (min + point[count-3]) / 2;
                }
                if (min2 == points[i])
                {
                    points[i] = (min2 + point[count - 2]) / 2;
                }
                if (min3 == points[i])
                {
                    points[i] = (min3 + point[midle+1]) / 2;
                }
            }

                return points;
        }
        // get the uniformity by 5 zones.
        public double getuniformity(double[, ,] XYZ, int pointNums)
        {
            int productType = 0;
            zoneresult zr = new zoneresult();            
            List<double> pointLv = new List<double>();
            string value = "";

            if (pointNums < 9) {
                pointNums = 9;
                productType = 0;
            }
            else {
                pointNums = 13;
                productType = 1;
            }

            zr.clear();

            for (int i = 0; i < pointNums; i++)
            {
               double[, ,] XYZValue = zr.XYZlocalzone(productType, (i + 1), 10, XYZ);
               double a = getlv(XYZValue);
               pointLv.Add(a);
               
            }
            if (log == null)
            {
                log = new Testlog();
            }
            pointLv = PointLV(pointLv);
            for (int i = 0; i < pointNums; i++)
            {
                value = string.Format("{0},{1}", value, pointLv[i]);
            }
          
            log.AppendCamareVaule(value);
            lvvalue = pointLv;
            double lvMax = pointLv.ToArray().Max();
            double lvMin = pointLv.ToArray().Min();
            double unif = lvMin / lvMax;

            return unif;
        }

        // get the uniformity by 5 zones.
        public double getuniformity(double[, ,] XYZ)
        {
            zoneresult zr = new zoneresult();
            double[, ,] XYZ1, XYZ2, XYZ3, XYZ4, XYZ5;
            double lv1, lv2, lv3, lv4, lv5;

            XYZ1 = zr.XYZlocalzone(1, 10, XYZ);
            lv1 = getlv(XYZ1);
            XYZ2 = zr.XYZlocalzone(2, 10, XYZ);
            lv2 = getlv(XYZ2);
            XYZ3 = zr.XYZlocalzone(3, 10, XYZ);
            lv3 = getlv(XYZ3);
            XYZ4 = zr.XYZlocalzone(4, 10, XYZ);
            lv4 = getlv(XYZ4);
            XYZ5 = zr.XYZlocalzone(5, 10, XYZ);
            lv5 = getlv(XYZ5);

            double lvmin = new double[]{lv1, lv2, lv3, lv4, lv5}.Min();
            double lvmax = new double[]{lv1, lv2, lv3, lv4, lv5}.Max();
            double unif = lvmin /lvmax;
            return unif;

        }

        public double getmura(double[, ,] XYZ)
        {
            double muraresult = 0;
            return muraresult;
        }
        
        public double[, ,] bmp2rgb(Bitmap processedBitmap)
        {
            int h = processedBitmap.Height;
            int w = processedBitmap.Width;
            this.rgbstr = new double[w, h, 3];

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    this.rgbstr[i, j, 0] = processedBitmap.GetPixel(i, j).R;
                    this.rgbstr[i, j, 1] = processedBitmap.GetPixel(i, j).G;
                    this.rgbstr[i, j, 2] = processedBitmap.GetPixel(i, j).B;
                }
            }

            return this.rgbstr;
        }

        public double[, ,] rgb2xyz(double[, ,] rgbstr)
        {
            int w = rgbstr.GetLength(0);
            int h = rgbstr.GetLength(1);
            double[, ,] xyzstr = new double[w, h, 3];
            var ccm = new[,]
               {
                    //{0.5767309, 0.2973769, 0.0270343},
                    //{0.1855540, 0.6273491, 0.0706872},
                    //{0.1881852, 0.0752741, 0.9911085}

                    //{0.8769193, 0.4521615, 0.0411056},    //b=1.5205
                    //{0.2821348, 0.9538843, 0.1074799},
                    //{0.2861356, 0.1144543, 1.5069805}

                    //{0.490, 0.310, 0.200},
                    //{0.177, 0.812, 0.011},
                    //{0.000, 0.010, 0.990},
                    
                    {0.6583150, 0.4164850, 0.2687000},          //b=1.3435
                    {0.2377995, 1.0914594, 0.0142411},
                    {0.0000000, 0.0134350, 1.3300650},
                   
                    //{0.53286545, 0.30368845, 0.11351715},
                    //{0.18127700, 0.71967455, 0.04084360},  
                    //{0.09409260, 0.04263705, 0.99055425},

                   // {0.75986613, 0.43305973, 0.16187546},   //b=1.426
                   // {0.25850100, 1.02625591, 0.05824297},  
                   // {0.13417605, 0.06080043, 1.41253036},

                    //{0.606881, 0.173505, 0.200336},
                    //{0.298912, 0.586611, 0.114478},
                    //{0.000000, 0.066097, 1.116157}

                    //{0.433910, 0.376220, 0.189860},
                    //{0.212649, 0.715169, 0.072182},
                    //{0.017756, 0.109478, 0.872915}
               };

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    double[] rgb = new double[] { rgbstr[i, j, 0], rgbstr[i, j, 1], rgbstr[i, j, 2] };
                    double[] xyz = MultiplyVector(ccm, rgb);
                    xyzstr[i, j, 0] = xyz[0];
                    xyzstr[i, j, 1] = xyz[1];
                    xyzstr[i, j, 2] = xyz[2];
                }
            }

            return xyzstr;
        }

        private double[,] MultiplyMatrix(double[,] A, double[,] B)
        {
            int rA = A.GetLength(0);
            int cA = A.GetLength(1);
            int rB = B.GetLength(0);
            int cB = B.GetLength(1);
            double temp = 0;
            double[,] kHasil = new double[rA, cB];
            if (cA != rB)
            {
                Console.WriteLine("matrik can't be multiplied !!");
                return null;
            }
            else
            {
                for (int i = 0; i < rA; i++)
                {
                    for (int j = 0; j < cB; j++)
                    {
                        temp = 0;
                        for (int k = 0; k < cA; k++)
                        {
                            temp += A[i, k] * B[k, j];
                        }
                        kHasil[i, j] = temp;
                    }
                }
                return kHasil;
            }
        }

        private double[] MultiplyVector(double[,] A, double[] B)
        {
            int rA = A.GetLength(0);
            int cA = A.GetLength(1);
            int rB = B.GetLength(0);
            double temp = 0;
            double[] kHasil = new double[rA];
            if (cA != rB)
            {
                Console.WriteLine("matrik can't be multiplied !!");
                return null;
            }
            else
            {
                for (int i = 0; i < rA; i++)
                {
                    temp = 0;
                    for (int k = 0; k < cA; k++)
                    {
                        temp += A[i, k] * B[k];
                    }
                    kHasil[i] = temp;

                }
                return kHasil;
            }
        }
    }
}
