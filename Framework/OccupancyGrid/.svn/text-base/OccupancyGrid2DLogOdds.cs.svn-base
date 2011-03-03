using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.DataTypes;
using Magic.Common.Sensors;
using Magic.Common;
using Magic.Common.Mapack;
using System.Threading;
using System.Diagnostics;


namespace Magic.OccupancyGrid
{
    /// <summary>
    /// This class provides 2d occupancy grids
    /// </summary>
    public class OccupancyGrid2DLogOdds : IDisposable /*IOccupancyGrid2DProcessor*/
    {
        double maxCellValue = 10;
        double minCellValue = -10;
        double decayFactor = 0.9;
        double gridDecay = .99;
        double lidarMaxPracticalRange = 50.0;
        bool running = true;

        //Required objects for making an occupancy grid and allowing other people to access our data
        IOccupancyGrid2D currentOccupancyGrid;
        IOccupancyGrid2D outputOccupancyGrid;

        //Current lidar and pose (hopefully these will be at the same time at some point)
        ILidarScan<ILidar2DPoint> curScan2D;

        //Object locks 
        object dataLocker = new object();
        RobotPose curRobotPose;
        SensorPose curLidarToBody;

        //Prior data time stamp
        double priorTimeStamp = 0;

        public event EventHandler<NewOccupancyGrid2DAvailableEventArgs> NewGridAvailable;

        public void SetRobotPose(RobotPose p)
        {
            lock (dataLocker) { curRobotPose = p; }
        }


        public void SetLidarPose(SensorPose p)
        {
            lock (dataLocker) { curLidarToBody = p; }
        }

        //Constructor 
        public OccupancyGrid2DLogOdds(IOccupancyGrid2D inputOccupancyGrid)
        {
            //Assign the occupancy grid we're going to process from the input occupancy grid
            currentOccupancyGrid = inputOccupancyGrid;
            outputOccupancyGrid = (IOccupancyGrid2D)currentOccupancyGrid.DeepCopy();
        }

        public OccupancyGrid2DLogOdds(IOccupancyGrid2D inputGrid, double decayFactor, double gridDecay)
        {
            this.currentOccupancyGrid = inputGrid;
            this.decayFactor = decayFactor;
            this.gridDecay = gridDecay;
            this.outputOccupancyGrid = (IOccupancyGrid2D)currentOccupancyGrid.DeepCopy();
        }

        public void UpdateLidarScan(ILidarScan<ILidar2DPoint> s)
        {
            lock (dataLocker)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                curScan2D = s;
                if (curRobotPose != null && curScan2D != null /*&& curRobotPose.timestamp != priorTimeStamp && curRobotPose.timestamp != 0*/)
                {
                    //Get the laser to body coordinate system (this is I for now)
                    Matrix4 Tlaser2body = Matrix4.FromPose(curLidarToBody);

                    //Get the body to global transformation
                    Matrix4 Tbody2global = Matrix4.FromPose(curRobotPose);

                    //Get a vector from the current lidar pose
                    Vector3 lidarInBody = new Vector3((double)curLidarToBody.x, (double)curLidarToBody.y, (double)curLidarToBody.z);

                    //Transform the sensor position in body coordinates to the sensor position in global coordinates
                    Vector3 lidarInGlobal = Tbody2global.TransformPoint(lidarInBody);

                    //Get the current grid indicies
                    int xLidarPoseIndex, yLidarPoseIndex;
                    currentOccupancyGrid.GetIndicies(lidarInGlobal.X, lidarInGlobal.Y, out xLidarPoseIndex, out yLidarPoseIndex);

                    //Find the cells corresponding to each LIDAR return and make a list of the cells that are clear from the sensor to that point
                    Dictionary<Vector2, Boolean> occupiedCellsThisScan = new Dictionary<Vector2, Boolean>(curScan2D.Points.Count());
                    Dictionary<Vector2, Boolean> clearCellsThisScan = new Dictionary<Vector2, Boolean>();

                    //Process each lidar return
                    foreach (ILidar2DPoint pt in curScan2D.Points)
                    {
                        if (pt.RThetaPoint.R < lidarMaxPracticalRange)
                        {
                            //Extract the lidar point in XYZ (laser coordinates)
                            Vector3 v3 = pt.RThetaPoint.ToVector3();

                            //Convert laser to body coordinate system
                            Vector3 vBody = Tlaser2body.TransformPoint(v3);

                            //Convert body to global cooridnate system
                            Vector3 vGlobal = Tbody2global.TransformPoint(vBody);

                            //Find the index of the laser return
                            int xLaserIndex, yLaserIndex;
                            currentOccupancyGrid.GetIndicies(vGlobal.X, vGlobal.Y, out xLaserIndex, out yLaserIndex);

                            //Add to the list of occupied cells
                            if (currentOccupancyGrid.CheckValidIdx(xLaserIndex, yLaserIndex))
                            {
                                occupiedCellsThisScan[new Vector2(xLaserIndex, yLaserIndex)] = true;
                                //occupiedCellsThisScan[new Vector2(xLaserIndex + 1, yLaserIndex)] = true;
                                //occupiedCellsThisScan[new Vector2(xLaserIndex, yLaserIndex - 1)] = true;
                                //occupiedCellsThisScan[new Vector2(xLaserIndex + 1, yLaserIndex - 1)] = true;
                            }

                        }
                    }

                    //Process each lidar return
                    foreach (ILidar2DPoint pt in curScan2D.Points)
                    {
                        if (pt.RThetaPoint.R < lidarMaxPracticalRange)
                        {
                            //Extract the lidar point in XYZ (laser coordinates)
                            Vector3 v3 = pt.RThetaPoint.ToVector3();

                            //Convert laser to body coordinate system
                            Vector3 vBody = Tlaser2body.TransformPoint(v3);

                            //Convert body to global cooridnate system
                            Vector3 vGlobal = Tbody2global.TransformPoint(vBody);

                            //Find the index of the laser return
                            int xLaserIndex, yLaserIndex;
                            currentOccupancyGrid.GetIndicies(vGlobal.X, vGlobal.Y, out xLaserIndex, out yLaserIndex);

                            //Ray trace between the two points performing the update
                            Raytrace(xLidarPoseIndex, yLidarPoseIndex, xLaserIndex, yLaserIndex, occupiedCellsThisScan, clearCellsThisScan);
                        }
                    }

                    //decay the whole grid
                    for (int i = 0; i < currentOccupancyGrid.NumCellX; i++)
                    {
                        for (int j = 0; j < currentOccupancyGrid.NumCellY; j++)
                        {
                            double value = currentOccupancyGrid.GetCellByIdx(i, j);
                            currentOccupancyGrid.SetCellByIdx(i, j, value *= gridDecay);

                        }
                    }

                    foreach (Vector2 cellIdx in occupiedCellsThisScan.Keys)
                    {
                        UpdateCellOccupied((int)cellIdx.X, (int)cellIdx.Y);
                    }

                    foreach (Vector2 cellIdx in clearCellsThisScan.Keys)
                    {
                        UpdateCellClear((int)cellIdx.X, (int)cellIdx.Y);
                    }



                    //Copy for the timestamp for the next iteration
                    priorTimeStamp = (double)curRobotPose.timestamp;

                }
//                Console.WriteLine("OG Took: " + sw.ElapsedMilliseconds);
            }//lock
            if (outputOccupancyGrid != null)
            {
                outputOccupancyGrid = (IOccupancyGrid2D)currentOccupancyGrid.DeepCopy();
                if (curRobotPose != null)
                    if (NewGridAvailable != null) NewGridAvailable(this, new NewOccupancyGrid2DAvailableEventArgs(GetOccupancyGrid(), curRobotPose.timestamp));
            }

        }
        public IOccupancyGrid2D GetOccupancyGrid()
        {
            lock (dataLocker)
            {
                return outputOccupancyGrid;
            }
        }

        void UpdateCellClear(int xIdx, int yIdx)
        {
            currentOccupancyGrid.SetCellByIdx(xIdx, yIdx, Math.Max(currentOccupancyGrid.GetCellByIdx(xIdx, yIdx) + Math.Log(decayFactor, Math.E), minCellValue));
        }
        void UpdateCellOccupied(int xIdx, int yIdx)
        {
            currentOccupancyGrid.SetCellByIdx(xIdx, yIdx, Math.Min(currentOccupancyGrid.GetCellByIdx(xIdx, yIdx) - Math.Log(decayFactor, Math.E), maxCellValue));
        }
        void Raytrace(int x0, int y0, int x1, int y1, Dictionary<Vector2, Boolean> occCells, Dictionary<Vector2, Boolean> clearCells)
        {
            //Change in x and y
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            //Initialize
            int x = x0;
            int y = y0;
            int n = 0 + dx + dy; //Add 1 to get the last cell
            int x_inc = (x1 > x0) ? 1 : -1;
            int y_inc = (y1 > y0) ? 1 : -1;
            int error = dx - dy;
            dx *= 2;
            dy *= 2;
            for (; n > 0; --n)
            {
                Vector2 curCell = new Vector2(x, y);
                if (occCells.Keys.Contains<Vector2>(curCell) == true)
                    return;
                else
                    clearCells[curCell] = true;
                //UpdateCellClear(x, y);
                if (error > 0)
                {
                    x += x_inc;
                    error -= dy;
                }
                else
                {
                    y += y_inc;
                    error += dx;
                }
            }
        }



        #region IDisposable Members

        public void Dispose()
        {
            running = false;
        }

        #endregion
    }


}
