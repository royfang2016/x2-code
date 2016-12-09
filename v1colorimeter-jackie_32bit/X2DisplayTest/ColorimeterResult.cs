using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace X2DisplayTest
{
    public enum ColorPanel
    {
        White,
        Black,
        Red,
        Green,
        Blue,
    }

    public class CIE1931Value
    {
        public double x { get; set; }
        public double y { get; set; }
        public double Y { get; set; }

        public override string ToString()
        {
            return string.Format("([{0}, {1}], {2})", Math.Round(x, 3), Math.Round(y, 3), Math.Round(Y, 3));
        }

        public CIE1931Value Copy()
        {
            CIE1931Value cie = new CIE1931Value();
            cie.x = this.x;
            cie.y = this.y;
            cie.Y = this.Y;

            return cie;
        }
    }

    public class ColorimeterResult
    {
        private Bitmap m_bitmap;
        private Bitmap m_bitmapDisp;
        private ColorPanel m_panel;
        private imagingpipeline m_pipeline;

        public double Luminance { get; private set; }
        public double Uniformity5 { get; private set; }
        public double Mura { get; private set; }
        public CIE1931Value CIE1931xyY { get; set; }

        public ColorimeterResult(Bitmap bitmap, Bitmap bitmapDisp)
        {
            this.m_bitmap = bitmap;
            this.m_bitmapDisp = bitmapDisp;
            m_pipeline = new imagingpipeline();
        }

        public ColorimeterResult(Bitmap bitmap, ColorPanel panel)
        {
            this.m_bitmap = bitmap;
            this.m_panel = panel;
            m_pipeline = new imagingpipeline();
            CIE1931xyY = new CIE1931Value();
        }

        public void Analysis(ref TestItem item, DUTclass.DUT dut)
        {
            int points = 9;

            if (dut is DUTclass.Hodor) {
                points = 13;
            }
            else if (dut is DUTclass.Bran) {
                points = 9;
            }


            if (m_bitmap != null) {
                double[, ,] rgb = m_pipeline.bmp2rgb(m_bitmap);
                double[, ,] XYZ = m_pipeline.rgb2xyz(rgb);
                double[] xyY = m_pipeline.getxyY(XYZ);
                item.TestNodes[3].Value = Math.Round(xyY[0], 5);
                item.TestNodes[4].Value = Math.Round(xyY[1], 5);
                item.TestNodes[0].Value = item.TestNodes[5].Value = Math.Round(xyY[2], 3);               

                if (m_bitmapDisp != m_bitmap)
                {
                    rgb = m_pipeline.bmp2rgb(m_bitmapDisp);
                    XYZ = m_pipeline.rgb2xyz(rgb);
                    //xyY = m_pipeline.getxyY(XYZ);
                }
                //item.TestNodes[5].Value = item.TestNodes[0].Value = Math.Round(m_pipeline.getlv(XYZ), 3);
                item.TestNodes[1].Value = Math.Round(m_pipeline.getuniformity(XYZ, points), 3);
                item.TestNodes[2].Value = Math.Round(m_pipeline.getmura(XYZ), 0);
                item.TestNodes[0].Value = item.TestNodes[5].Value = Math.Round(m_pipeline.lvValue.Max(), 3);   
            }
        }

        public void Analysis()
        {
            if (m_bitmap != null)
            {
                double[, ,] rgb = m_pipeline.bmp2rgb(m_bitmap);
                double[, ,] XYZ = m_pipeline.rgb2xyz(rgb);

                switch (m_panel)
                {
                    case ColorPanel.White:
                    case ColorPanel.Black:
                        this.Luminance = m_pipeline.getlv(XYZ);
                        this.Uniformity5 = m_pipeline.getuniformity(XYZ, 9);
                        this.Mura = m_pipeline.getmura(XYZ);
                        break;
                    case ColorPanel.Red:
                    case ColorPanel.Green:
                    case ColorPanel.Blue:
                        {
                            double[] xyY = m_pipeline.getxyY(XYZ);
                            CIE1931xyY.x = xyY[0];
                            CIE1931xyY.y = xyY[1];
                            CIE1931xyY.Y = xyY[2];
                        }
                        break;
                }
            }
        }
    }
}
