using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CA200SRVRLib;


namespace X2DisplayTest
{
    public class KonicaCa310
    {
        public KonicaCa310()
        {
            objCa200 = new Ca200();
        }

        public enum Ca310TestMode
        {
            DSP_LXY = 0,
            DSP_DUV = 1,
            DSP_ANL = 2,
            DSP_ANLG = 3,
            DSP_ANLR = 4,
            DSP_PUV = 5,
            DSP_FMA = 6,
            DSP_XYZC = 7,
            DSP_JEITA = 8,
            DSP_XYZ = 9,
        }

        private ICa200 objCa200;
        private Ca objCa;
        private Probe objProbe;

        private string errorInfo;
        public string ErrorMessage
        {
            get
            {
                return errorInfo;
            }
        }

        public void Initiaze()
        {
            try
            {
                errorInfo = "";
                objCa200.AutoConnect();
                objCa = objCa200.SingleCa;
                objProbe = objCa.SingleProbe;
                this.ChangeMode(Ca310TestMode.DSP_LXY);
            }
            catch
            {
                errorInfo = "Can't init Ca310.";
            }
        }

        public void Zero()
        {
            if (objCa == null)
            {
                this.Initiaze();
            }

            try
            {
                errorInfo = "";
                objCa.CalZero();
            }
            catch (Exception e)
            {
                errorInfo = e.Message;
            }
        }

        public CIE1931Value CIE1931xyY { get; private set; }
        public CIE1931Value GetCa310Data()
        {
            if (CIE1931xyY == null)
            {
                CIE1931xyY = new CIE1931Value();
            }
            CIE1931xyY.x = CIE1931xyY.y = CIE1931xyY.Y = 0;

             string  result = Measure();
            if (errorInfo == "")
            {
                if (!string.IsNullOrEmpty(result))
                {
                    string[] arrayStr = result.Split(new char[] { ',' });

                    if (arrayStr.Length == 3)
                    {
                        CIE1931xyY.Y = double.Parse(arrayStr[0].Substring(3));
                        CIE1931xyY.x = double.Parse(arrayStr[1].Substring(3));
                        CIE1931xyY.y = double.Parse(arrayStr[2].Substring(3));
                    }
                }
            }
            else
            {
                errorInfo = result;
            }
   

            return CIE1931xyY;
        }

        public string Measure()
        {
            string result = "";

            if (objCa == null)
            {
                this.Initiaze();
            }

            try
            {
                errorInfo = "";
                objCa.Measure();

                switch (objCa.DisplayMode)
                {
                    case (int)Ca310TestMode.DSP_LXY:
                        result = string.Format("lv={0},sx={1},sy={2}", objProbe.Lv, objProbe.sx, objProbe.sy);
                        break;
                    case (int)Ca310TestMode.DSP_XYZ:
                        result = string.Format("X={0},Y={1},Z={2}", objProbe.X, objProbe.Y, objProbe.Z);
                        break;
                }
            }
            catch
            {
                errorInfo = "Can't measure. Is the swicth at the measure.";
            }

            return result;
        }

        public void ChangeMode(Ca310TestMode mode)
        {
            objCa.DisplayMode = (int)mode;
        }
    }
}
