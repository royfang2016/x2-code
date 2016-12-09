//=============================================================================
// Main zone analysis. display test revolved with zones. This class will resize
// XYZ matrix (which is already in mm binning level for this case) to different 
// zones and perform uniformity analysis.
//=============================================================================

// Location 1 : Top left zone with 10% padding
// Location 2:  Top right zone with 10% padding
// Location 3:  Center Zone
// Location 4:  Bottom right zone with 10% padding
// Location 5:  Bottom left zone with 10% padding

// If location is a point pair to tell the relative XY coordinates in mm level. To be done later.



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge; 

namespace X2DisplayTest
{
    class zoneresult
    {
        private int mLocation;
        public int Location
        {
            get { return mLocation; }
            set { mLocation = value; }
        }

        private int mzonesize;
        public int zonesize
        {
            get { return mzonesize; }
            set { zonesize = value; }
        }

        private double[, ,] XYZlocal; // XYZ tristimulus value matrix in this zone
        private List<IntPoint> Points = new List<IntPoint>();
        System.Drawing.Point pp, pp1, pp2, pp3, pp4 = new System.Drawing.Point();

        // get the local zone XYZ matrix by inputting the predefined zone. 
        public double[, ,] XYZlocalzone(int productType, int Location, int zonesize, double[, ,] XYZ)
        {
            mLocation = Location;
            int[] xy_index = zoneindex(productType, mLocation, zonesize, XYZ);
            XYZlocal = cropXYZ(xy_index[0], xy_index[1], xy_index[2], xy_index[3], XYZ);
            return XYZlocal;
        }

        // get the local zone XYZ matrix by inputting the predefined zone. 
        public double[, ,] XYZlocalzone(int Location, int zonesize, double[, ,] XYZ)
        {
            mLocation = Location;
            int[] xy_index = zoneindex(0, mLocation, zonesize, XYZ);
            XYZlocal = cropXYZ(xy_index[0], xy_index[1], xy_index[2], xy_index[3], XYZ);
            return XYZlocal;
        }

        // get the local XYZ matrix by inputting the starting and endding xy cooordinate in mm 
        // same as the XYZ raw in this study.
        private double[,,] cropXYZ(int x_start, int x_end, int y_start, int y_end, double[, ,] XYZ)
        {

            double[, ,] XYZlocal = new double[x_end - x_start, y_end - y_start, XYZ.GetLength(2)];

            for (int k = 0; k < XYZ.GetLength(2); k++)
            {
                for (int i = x_start; i < x_end; i++)
                {
                    for (int j = y_start; j < y_end; j++)
                    {
                        XYZlocal[i - x_start, j - y_start, k] = XYZ[i, j, k];
                    }
                }
            }
            return XYZlocal;

        }

        private int col3Point = 1, col2Point = 1;
        private int currentRow = 0, currentColumn = 0;

        public void clear()
        {
            col2Point = col3Point = 1;
            currentRow = currentColumn = 0;
        }

        private int[] Index13(int location)
        {
            switch(location)
            {
                case 1:
                   // return new int[] { 30, 30 };
                    return new int[] { 50, 50 };
                case 2:
                    return new int[] { 480, 50 };
                case 3:
                    return new int[] { 910, 50 };
                case 4:
                    return new int[] { 250, 145 };
                case 5:
                    return new int[] { 710, 145 };
                case 6:
                    return new int[] { 50, 270 };
                case 7:
                    return new int[] { 480, 270 };
                case 8:
                    return new int[] { 910, 270 };
                case 9:
                    return new int[] { 250, 395 };
                case 10:
                    return new int[] { 710, 395 };
                case 11:
                    return new int[] { 50, 490 };
                case 12:
                    return new int[] { 480, 490 };
                case 13:
                    return new int[] { 910, 490 };
            }
            return new int[] { 30, 30 };
         
        }

        /*
        private int[] zoneindex(int productType, int Location, int zonesize, double[, ,] XYZ)
        {

            int w = XYZ.GetLength(0);
            int h = XYZ.GetLength(1);
            int x_start, y_start;
            int[] zoneindex = new int[4]; // start x, start y, and end x, end y;

            if (productType == 0)
            {
                int row = (Location - 1) / 3;
                int column = (Location - 1) % 3;

                x_start = Convert.ToInt32(System.Math.Round(1.0 * (2 * column + 1) * w / 6));
                y_start = Convert.ToInt32(System.Math.Round(1.0 * (2 * row + 1) * h / 6));

                zoneindex[0] = x_start;
                zoneindex[1] = x_start + zonesize;
                zoneindex[2] = y_start;
                zoneindex[3] = y_start + zonesize;

            }
            else
            {
                x_start = Index13(Location)[0];
                y_start = Index13(Location)[1];

                zoneindex[0] = x_start;
                zoneindex[1] = x_start + zonesize;
                zoneindex[2] = y_start;
                zoneindex[3] = y_start + zonesize;
            }

            return zoneindex;
        }

      */
        private int[] zoneindex(int productType, int Location, int zonesize, double[, ,] XYZ)
        {

            int w = XYZ.GetLength(0);
            int h = XYZ.GetLength(1);
            int x_start, y_start;
            int[] zoneindex = new int[4]; // start x, start y, and end x, end y;

            if (productType == 0) {
                int row = (Location - 1) / 3;
                int column = (Location - 1) % 3;

                x_start = Convert.ToInt32(System.Math.Round(1.0 * (2 * column + 1) * w / 6));
                y_start = Convert.ToInt32(System.Math.Round(1.0 * (2 * row + 1) * h / 6));

                zoneindex[0] = x_start;
                zoneindex[1] = x_start + zonesize;
                zoneindex[2] = y_start;
                zoneindex[3] = y_start + zonesize;

            }
            else {
                int row = 0, column = 0;
                int loc = Location + Location / 6;

                if (Location > 10 && Location < 13)
                {
                    loc++;
                }

                row = (loc - 1) / 3;
                //column = (loc - 1) % 3;

                if (row % 2 == 0) {
                    x_start = Convert.ToInt32(System.Math.Round(1.0 * (4 * currentColumn + 1) * w / 10));
                    y_start = Convert.ToInt32(System.Math.Round(1.0 * col3Point * h / 10));
                }
                else {
                    x_start = Convert.ToInt32(System.Math.Round(1.0 * (2 * currentColumn + 1) * w / 4));
                    y_start = Convert.ToInt32(System.Math.Round(1.0 * col2Point * h / 4));
                }
                currentColumn++;

                if (row % 2 == 0 && currentColumn > 2)
                {
                    currentColumn = 0;
                }
                else if (row % 2 == 1 && currentColumn > 1)
                {
                    currentColumn = 0;
                }

                if (Location % 5 == 0)
                {
                    col3Point += 4;
                }

                if (Location % 5 == 0)
                {
                    col2Point += 2;
                }

                zoneindex[0] = x_start;
                zoneindex[1] = x_start + zonesize;
                zoneindex[2] = y_start;
                zoneindex[3] = y_start + zonesize;
            }

            return zoneindex;
        }
     
        // derive the xy coordinate in mm from the predefined zone location and zonesize.
        private int[] zoneindex(int Location, int zonesize, double[, ,] XYZ)
        {
            int w = XYZ.GetLength(0);
            int h = XYZ.GetLength(1);
            int x_start, y_start;
            int[] zoneindex = new int[4]; // start x, start y, and end x, end y;
            if (Location == 1)
            {
                x_start = Convert.ToInt32(System.Math.Round(w * 0.1));
                y_start = Convert.ToInt32(System.Math.Round(h * 0.1));
            }
            else if (Location == 2)
            {
                x_start = Convert.ToInt32(System.Math.Round(w * 0.9)) - zonesize;
                y_start = Convert.ToInt32(System.Math.Round(h * 0.1));
            }
            else if (Location == 3)
            {
                x_start = Convert.ToInt32(System.Math.Round(w * 0.9)) - zonesize;
                y_start = Convert.ToInt32(System.Math.Round(h * 0.9)) - zonesize;
            }
            else if (Location == 4)
            {
                x_start = Convert.ToInt32(System.Math.Round(w * 0.1));
                y_start = Convert.ToInt32(System.Math.Round(h * 0.9)) - zonesize;
            }
            else if (Location == 5)
            {
                x_start = Convert.ToInt32(System.Math.Round(w * 0.5)) - zonesize/2;
                y_start = Convert.ToInt32(System.Math.Round(h * 0.5)) - zonesize/2;
            }
            else
            {
                x_start = 1;
                y_start = 1;
                Console.WriteLine("Location {0} is out of the predefined range 1-5:", Location);
                Console.WriteLine("Done! Press enter to exit...");
                Console.ReadLine();
            }

            zoneindex[0] = x_start;
            zoneindex[1] = x_start + zonesize;
            zoneindex[2] = y_start;
            zoneindex[3] = y_start + zonesize;

            return zoneindex;
        }

        // derive the point coordinate in pixel from the predefined zone location and zonesize.
        // will be used for UI drawing if needed
        public List<IntPoint> zonecorners(int productType, int Location, int zonesize, double[, ,] XYZ)
        {
            int w = XYZ.GetLength(0);
            int h = XYZ.GetLength(1);
            int x_start, y_start;
            int[] zoneindex = new int[8]; // start x, start y, and end x, end y;

            if (productType == 0) {
                int row = (Location - 1) / 3;
                int column = (Location - 1) % 3;

                x_start = Convert.ToInt32(System.Math.Round(1.0 * (2 * column + 1) * w / 6));
                y_start = Convert.ToInt32(System.Math.Round(1.0 * (2 * row + 1) * h / 6));
            }
            else
            {
                x_start = Index13(Location)[0];
                y_start = Index13(Location)[1];

               /*

                    int row = 0, column = 0;
                    int loc = Location + Location / 6;

                    if (Location > 10 && Location < 13)
                    {
                        loc++;
                    }

                    row = (loc - 1) / 3;
                    //column = (loc - 1) % 3;

                    if (row % 2 == 0)
                    {
                        x_start = Convert.ToInt32(System.Math.Round(1.0 * (4 * currentColumn + 1) * w / 10));
                        y_start = Convert.ToInt32(System.Math.Round(1.0 * col3Point * h / 10));
                    }
                    else
                    {
                        x_start = Convert.ToInt32(System.Math.Round(1.0 * (2 * currentColumn + 1) * w / 4));
                        y_start = Convert.ToInt32(System.Math.Round(1.0 * col2Point * h / 4));
                    }
                    currentColumn++;

                    if (row % 2 == 0 && currentColumn > 2)
                    {
                        currentColumn = 0;
                    }
                    else if (row % 2 == 1 && currentColumn > 1)
                    {
                        currentColumn = 0;
                    }

                    if (Location % 5 == 0)
                    {
                        col3Point += 4;
                    }

                    if (Location % 5 == 0)
                    {
                        col2Point += 2;
                    }
               */
            }

            zoneindex[0] = x_start;
            zoneindex[1] = y_start;
            zoneindex[2] = x_start + zonesize;
            zoneindex[3] = y_start;
            zoneindex[4] = x_start;
            zoneindex[5] = y_start + zonesize;
            zoneindex[6] = x_start + zonesize;
            zoneindex[7] = y_start + zonesize;

            Points.Add(new IntPoint(zoneindex[0], zoneindex[1]));
            Points.Add(new IntPoint(zoneindex[2], zoneindex[3]));
            Points.Add(new IntPoint(zoneindex[6], zoneindex[7]));
            Points.Add(new IntPoint(zoneindex[4], zoneindex[5]));

            return Points;
        }
    }
}
