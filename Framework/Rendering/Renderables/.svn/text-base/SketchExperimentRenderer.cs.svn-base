using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Magic.Common.Shapes;
using Magic.Common;
using Magic.Common.Mapack;
using System.Windows.Forms;
using Magic.Rendering.Renderables;
using Magic.Rendering;

namespace Magic.Rendering
{
	public class SketchExperimentRenderer : IRender
	{
		private Color color;
		private string name = "Placeholder Name";
		private int expNum;
		private List<List<PointF>> wallPoints = new List<List<PointF>>();
		List<PointF> homeBase = new List<PointF>();
		List<FakeObjectRenderer> fakeObjectList = new List<FakeObjectRenderer>();
		//public event EventHandler<AddObjectsEventArgs> AddObjectsToRenderer;

		List<Color> colorList = new List<Color>(3) { Color.Green, Color.Blue, Color.Red };
		List<Color> colorList2 = new List<Color>(3) { Color.Red, Color.BlueViolet, Color.DarkTurquoise };
		List<PointF> polygonPoints = new List<PointF>();
		private Random rand = new Random();

		public SketchExperimentRenderer(int expNum)
		{
			this.color = Color.DarkSlateGray;
			this.expNum = expNum;
			BuildWalls();
			PlaceObjects();

		}

		public SketchExperimentRenderer(int expNum, Color color)
		{
			this.color = color;
			this.expNum = expNum;
			BuildWalls();
			PlaceObjects();
		}

		private void BuildWalls()
		{
			List<PointF> wallSegment = new List<PointF>();
			wallSegment.Add(new PointF(9.5f, -2.5f));
			wallSegment.Add(new PointF(9.5f, 2.8f));
			wallSegment.Add(new PointF(-3.5f, 3.0f));
			wallSegment.Add(new PointF(-3.5f, -2.5f));
			wallSegment.Add(new PointF(9.5f, -2.5f));
			wallPoints.Add(wallSegment);

			List<PointF> wallSegment2 = new List<PointF>();
			wallSegment2.Add(new PointF(9.5f, 0f));
			wallSegment2.Add(new PointF(8.6f, 1f));
			wallSegment2.Add(new PointF(8.3f, 2.8f));
			wallPoints.Add(wallSegment2);

			List<PointF> wallSegment3 = new List<PointF>();
			wallSegment3.Add(new PointF(1.2f, 1.3f));
			wallSegment3.Add(new PointF(2f, 0.25f));
			wallSegment3.Add(new PointF(3.5f, 0.25f));
			wallPoints.Add(wallSegment3);

			List<PointF> wallSegment4 = new List<PointF>();
			wallSegment4.Add(new PointF(0.9f, -0.7f));
			wallSegment4.Add(new PointF(2f, 0.25f));
			wallPoints.Add(wallSegment4);

			List<PointF> wallSegment5 = new List<PointF>();
			wallSegment5.Add(new PointF(6.1f, -0.8f));
			wallSegment5.Add(new PointF(5.9f, -0.6f));
			wallSegment5.Add(new PointF(6.8f, 0.4f));
			wallSegment5.Add(new PointF(7f, .2f));
			wallPoints.Add(wallSegment5);

			homeBase.Add(new PointF(7.5f, -2.5f));
			homeBase.Add(new PointF(7.5f, -1.5f));
			homeBase.Add(new PointF(8.5f, -.5f));
			homeBase.Add(new PointF(9.5f, -.5f));

		}
		private void PlaceObjects()
		{

			if (expNum == 0)
			{


				List<PointF> polyPoints1 = new List<PointF>();
				polyPoints1.Add(new PointF(8.0f, 0.8f));
				polyPoints1.Add(new PointF(7.3f, 2.6f));
				polyPoints1.Add(new PointF(4f, 2.6f));
				polyPoints1.Add(new PointF(4f, 0.8f));
				polyPoints1.Add(new PointF(8.0f, 0.8f));

				List<PointF> polyPoints2 = new List<PointF>();
				polyPoints2.Add(new PointF(3.5f, .05f));
				polyPoints2.Add(new PointF(5.5f, -2.3f));
				polyPoints2.Add(new PointF(1.5f, -2.3f));
				polyPoints2.Add(new PointF(3.5f, .05f));

				FakeObjectRenderer circle1 = new FakeObjectRenderer("circle", new PointF(0f, 2f), " ", Color.Green);
				fakeObjectList.Add(circle1);

				FakeObjectRenderer box1 = new FakeObjectRenderer("box", new PointF(-2.5f, -1f), " ", Color.Red);
				fakeObjectList.Add(box1);

				FakeObjectRenderer area1 = new FakeObjectRenderer("fill polygon", new PointF(0f, 0f), " ", Color.Red, polyPoints1);
				fakeObjectList.Add(area1);

				FakeObjectRenderer area2 = new FakeObjectRenderer("fill polygon", new PointF(0f, 0f), " ", Color.Green, polyPoints2);
				fakeObjectList.Add(area2);
			}
			else if (expNum == 2)
			{

				List<PointF> polyPoints1 = new List<PointF>();
				polyPoints1.Add(new PointF(1.3f, 1.5f));
				polyPoints1.Add(new PointF(1.3f, 2.80f));
				polyPoints1.Add(new PointF(3.5f, 2.75f));
				polyPoints1.Add(new PointF(3.5f, 0.4f));
				polyPoints1.Add(new PointF(2.1f, 0.4f));
				polyPoints1.Add(new PointF(1.3f, 1.5f));

				List<PointF> polyPoints2 = new List<PointF>();
				polyPoints2.Add(new PointF(-3.2f, -2.3f));
				polyPoints2.Add(new PointF(-3.2f, 1f));
				polyPoints2.Add(new PointF(0f, -2.3f));
				polyPoints2.Add(new PointF(-3.2f, -2.3f));

				FakeObjectRenderer circle1 = new FakeObjectRenderer("circle", new PointF(5.5f, -1.5f), " ", Color.Red);
				fakeObjectList.Add(circle1);

				FakeObjectRenderer box1 = new FakeObjectRenderer("box", new PointF(1f, 1.6f), " ", Color.Green);
				fakeObjectList.Add(box1);

				FakeObjectRenderer area1 = new FakeObjectRenderer("fill polygon", new PointF(0f, 0f), " ", Color.Red, polyPoints1);
				fakeObjectList.Add(area1);

				FakeObjectRenderer area2 = new FakeObjectRenderer("fill polygon", new PointF(0f, 0f), " ", Color.Green, polyPoints2);
				fakeObjectList.Add(area2);

				//FakeObjectRenderer triangle1 = new FakeObjectRenderer("triangle", new PointF(6f, 2f), " ", Color.Green);
				//fakeObjectList.Add(triangle1);
			}
			else if (expNum == 1)
			{
				List<PointF> polyPoints1 = new List<PointF>();
				polyPoints1.Add(new PointF(1f, -2.3f));
				polyPoints1.Add(new PointF(1f, -.9f));
				polyPoints1.Add(new PointF(2.1f, .1f));
				polyPoints1.Add(new PointF(3f, -2.3f));
				polyPoints1.Add(new PointF(1f, -2.3f));

				List<PointF> polyPoints2 = new List<PointF>();
				polyPoints2.Add(new PointF(-3.2f, .25f));
				polyPoints2.Add(new PointF(-3.2f, 2.8f));
				polyPoints2.Add(new PointF(1.8f, .25f));
				polyPoints2.Add(new PointF(-3.2f, .25f));

				FakeObjectRenderer circle1 = new FakeObjectRenderer("box", new PointF(7.5f, .6f), " ", Color.Red);
				fakeObjectList.Add(circle1);
				FakeObjectRenderer box1 = new FakeObjectRenderer("circle", new PointF(3.8f, -1.7f), " ", Color.Green);
				fakeObjectList.Add(box1);
				FakeObjectRenderer area1 = new FakeObjectRenderer("fill polygon", new PointF(0f, 0f), " ", Color.Red, polyPoints1);
				fakeObjectList.Add(area1);
				FakeObjectRenderer area2 = new FakeObjectRenderer("fill polygon", new PointF(0f, 0f), " ", Color.Green, polyPoints2);
				fakeObjectList.Add(area2);
				//FakeObjectRenderer triangle1 = new FakeObjectRenderer("triangle", new PointF(8f, 8f), " ", Color.DarkSlateGray);
				//fakeObjectList.Add(triangle1);
			}
			else if (expNum == 3)
			{
				List<PointF> polyPoints2 = new List<PointF>();
				polyPoints2.Add(new PointF(1f, -2.3f));
				polyPoints2.Add(new PointF(1f, -.9f));
				polyPoints2.Add(new PointF(2.1f, .1f));
				polyPoints2.Add(new PointF(3f, -2.3f));
				polyPoints2.Add(new PointF(1f, -2.3f));

				List<PointF> polyPoints1 = new List<PointF>();
				polyPoints1.Add(new PointF(-3.2f, .25f));
				polyPoints1.Add(new PointF(-3.2f, 2.8f));
				polyPoints1.Add(new PointF(1.8f, .25f));
				polyPoints1.Add(new PointF(-3.2f, .25f));

				FakeObjectRenderer circle1 = new FakeObjectRenderer("box", new PointF(3.8f, -1.7f), " ", Color.Red);
				fakeObjectList.Add(circle1);
				FakeObjectRenderer box1 = new FakeObjectRenderer("circle", new PointF(7.5f, .6f), " ", Color.Green);
				fakeObjectList.Add(box1);
				FakeObjectRenderer area1 = new FakeObjectRenderer("fill polygon", new PointF(0f, 0f), " ", Color.Red, polyPoints1);
				fakeObjectList.Add(area1);
				FakeObjectRenderer area2 = new FakeObjectRenderer("fill polygon", new PointF(0f, 0f), " ", Color.Green, polyPoints2);
				fakeObjectList.Add(area2);
			}
			else if (expNum == 100)		// DATA COLLECT TRAINING
			{
				wallPoints.Clear();
				homeBase.Clear();

				// path
				fakeObjectList.Add(new FakeObjectRenderer("path start string", new PointF(-.5f, 1f), " ", Color.Blue));
				fakeObjectList.Add(new FakeObjectRenderer("path end string", new PointF(4.5f, 0f), " ", Color.Blue));

				fakeObjectList.Add(new FakeObjectRenderer("circle string", new PointF(3f, 3f), " ", Color.DarkSlateGray));
				//wallPoints.Add(new List<PointF>(2) { new PointF(4f,-2f), new PointF(3f,3f) });

				// box
				fakeObjectList.Add(new FakeObjectRenderer("box string", new PointF(6.5f, -1f), " ", Color.DarkSlateGray));

				// triangle
				fakeObjectList.Add(new FakeObjectRenderer("triangle string", new PointF(-1.5f, 3f), " ", Color.DarkSlateGray));

				// arrow
				double angle = 7 * Math.PI / 4;
				fakeObjectList.Add(new FakeObjectRenderer("arrow string", new PointF(7.5f, 2.5f), " ", Color.Red, angle, 1));

				// waypoint
				fakeObjectList.Add(new FakeObjectRenderer("x string", new PointF(8.5f, -.5f), " ", Color.DarkSlateGray));

				// important
				fakeObjectList.Add(new FakeObjectRenderer("important string", new PointF(1.5f, 0f), " ", Color.DarkSlateGray));

				// spiral
				fakeObjectList.Add(new FakeObjectRenderer("spiral string", new PointF(2.5f, -2f), " ", Color.DarkSlateGray));

				// zone
				polygonPoints.Add(new PointF(-3f, -2.3f));
				polygonPoints.Add(new PointF(0f, -2.3f));
				polygonPoints.Add(new PointF(-3f, 2f));
				polygonPoints.Add(new PointF(-3f, -2.3f));
				fakeObjectList.Add(new FakeObjectRenderer("polygon string", new PointF(-2f, -1f), " ", Color.DarkSlateGray, polygonPoints));

			}
			else if (expNum > 100)		// DATA COLLECT ONLY
			{
				wallPoints.Clear();
				homeBase.Clear();
				List<Vector2> allObjects = new List<Vector2>();
				int numObjects = 0;
				int aaa = 0;

				double locX;
				double locY;
				int remainder;
				double asdf = Math.DivRem((expNum - 100), 4, out remainder);

				int numPaths;
				int numArrows;
				int numBoxes;
				int numSpirals;
				int numTriangles;
				int numImportants;
				int numAreas;
				int numWalls;
				int numWaypoints;
				int numCircles;

				if (remainder == 2)
				{
					numCircles = 1;
					numPaths = 1;
					numAreas = 1;
					numArrows = 0;
					numBoxes = 1;
					numSpirals = 0;
					numImportants = 1;
					numTriangles = 0;
					numWaypoints = 2;
					numWalls = 0;// rand.Next(1, 3);
				}
				else if (remainder == 1)
				{
					numCircles = 0;
					numPaths = 0;
					numAreas = 1;
					numArrows = 2;
					numBoxes = 1;
					numSpirals = 0;
					numImportants = 1;
					numTriangles = 2;
					numWaypoints = 0;
					numWalls = 0;

				}
				else if (remainder == 3)
				{
					numCircles = 1;
					numPaths = 2;
					numAreas = 0;
					numArrows = 1;
					numBoxes = 0;
					numSpirals = 1;
					numImportants = 0;
					numTriangles = 1;
					numWaypoints = 0;
					numWalls = 0;//rand.Next(1, 3);

				}
				else
				{
					numCircles = 1;
					numPaths = 0;
					numAreas = 1;
					numArrows = 0;
					numBoxes = 1;
					numSpirals = 2;
					numImportants = 1;
					numTriangles = 0;
					numWaypoints = 1;
					numWalls = 0;

				}

				//numPaths = numPaths + rand.Next(0, 2);
				//numObjects = numPaths;
				//aaa = numObjects;
				//while (numObjects < 1)
				//{
				//    numWaypoints = numWaypoints + rand.Next(0, 2);
				//    numArrows = numArrows + rand.Next(0, 2);
				//    numBoxes = numBoxes + rand.Next(0, 2);

				//    numObjects = numPaths + numWaypoints + numArrows + numBoxes;
				//}
				//aaa = numObjects;
				//while (numObjects < 3)
				//{
				//    numSpirals = numSpirals + rand.Next(0, 2);
				//    numTriangles = numTriangles + rand.Next(0, 2);

				//    numObjects = numPaths + numWaypoints + numArrows + numBoxes + numSpirals + numTriangles;
				//}
				//aaa = numObjects;
				//while (numObjects < 4)
				//{
				//    numImportants = numImportants + rand.Next(0, 2);
				//    numAreas = numAreas + rand.Next(0, 2);

				//    numObjects = numPaths + numWaypoints + numArrows + numBoxes + numSpirals + numTriangles + numImportants + numAreas;
				//}
				//aaa = numObjects;

				for (int i = 0; i < numAreas; i++)
				{
					//List<Vector2> areaList = new List<Vector2>();
					locX = new double();
					locY = new double();
					int redo = 1;
					while (redo == 1)
					{
						redo = 0;
						locX = -3.1 + (rand.NextDouble() * (8.8 + 3.1));
						locY = -2.1 + (rand.NextDouble() * (2.6 + 2.1));

						//foreach (Vector2 v in areaList)
						//{
						//    if (v.DistanceTo(new Vector2(locX, locY)) < 2)
						//    {
						//        redo = 1;
						//    }
						//}
						if (allObjects != null)
						{
							foreach (Vector2 v in allObjects)
							{
								if (v.DistanceTo(new Vector2(locX, locY)) < 1)
								{
									redo = 1;
								}
							}
						}
					}

					allObjects.Add(new Vector2(locX, locY));
					//areaList.Add(new Vector2(locX, locY));
					//redo = 1;
					//double locX2 = new double();
					//double locY2 = new double();


					//while (redo == 1)
					//{
					//    redo = 0;
					//    locX2 = -3.1 + (rand.NextDouble() * (8.8 + 3.1));
					//    locY2 = -2.1 + (rand.NextDouble() * (2.6 + 2.1));

					//    foreach (Vector2 v in areaList)
					//    {
					//        if (v.DistanceTo(new Vector2(locX2, locY2)) < 2)
					//        {
					//            redo = 1;
					//        }
					//    }
					//    if (allObjects != null)
					//    {
					//        foreach (Vector2 v in allObjects)
					//        {
					//            if (v.DistanceTo(new Vector2(locX2, locY2)) < 1)
					//            {
					//                redo = 1;
					//            }
					//        }
					//    }
					//}
					//allObjects.Add(new Vector2(locX2, locY2));
					//areaList.Add(new Vector2(locX2, locY2));
					//redo = 1;
					//double locX3 = new double();
					//double locY3 = new double();

					//while (redo == 1)
					//{
					//    redo = 0;
					//    locX3 = -3.1 + (rand.NextDouble() * (8.8 + 3.1));
					//    locY3 = -2.1 + (rand.NextDouble() * (2.6 + 2.1));

					//    foreach (Vector2 v in areaList)
					//    {
					//        if (v.DistanceTo(new Vector2(locX3, locY3)) < 2)
					//        {
					//            redo = 1;
					//        }
					//    }
					//    if (allObjects != null)
					//    {
					//        foreach (Vector2 v in allObjects)
					//        {
					//            if (v.DistanceTo(new Vector2(locX3, locY3)) < 1)
					//            {
					//                redo = 1;
					//            }
					//        }
					//    }
					//}
					//allObjects.Add(new Vector2(locX3, locY3));
					//polygonPoints.Add(new PointF((float)locX, (float)locY));
					//polygonPoints.Add(new PointF((float)locX2, (float)locY2));
					//polygonPoints.Add(new PointF((float)locX3, (float)locY3));
					//polygonPoints.Add(new PointF((float)locX, (float)locY));

					//fakeObjectList.Add(new FakeObjectRenderer("polygon", new PointF(0, 0), " ", Color.DarkSlateGray, polygonPoints));
					fakeObjectList.Add(new FakeObjectRenderer("polygon string", new PointF((float)locX, (float)locY), " ", Color.DarkSlateGray));
				}

				for (int i = 0; i < numBoxes; i++)
				{
					int redo = 1;
					locX = new double();
					locY = new double();

					while (redo == 1)
					{
						redo = 0;
						locX = -3.1 + (rand.NextDouble() * (8.8 + 3.1));
						locY = -2.1 + (rand.NextDouble() * (2.6 + 2.1));

						if (allObjects != null)
						{
							foreach (Vector2 v in allObjects)
							{
								if (v.DistanceTo(new Vector2(locX, locY)) < 1)
								{
									redo = 1;
								}
							}
						}
					}

					fakeObjectList.Add(new FakeObjectRenderer("box string", new PointF((float)locX, (float)locY), " ", Color.DarkSlateGray));
					allObjects.Add(new Vector2(locX, locY));
				}


				for (int i = 0; i < numSpirals; i++)
				{
					int redo = 1;
					locX = new double();
					locY = new double();

					while (redo == 1)
					{
						redo = 0;
						locX = -3.1 + (rand.NextDouble() * (8.8 + 3.1));
						locY = -2.1 + (rand.NextDouble() * (2.6 + 2.1));

						if (allObjects != null)
						{
							foreach (Vector2 v in allObjects)
							{
								if (v.DistanceTo(new Vector2(locX, locY)) < 1)
								{
									redo = 1;
								}
							}
						}
					}
					fakeObjectList.Add(new FakeObjectRenderer("spiral string", new PointF((float)locX, (float)locY), " ", Color.DarkSlateGray));
					allObjects.Add(new Vector2(locX, locY));
				}


				for (int i = 0; i < numTriangles; i++)
				{
					int redo = 1;
					locX = new double();
					locY = new double();

					while (redo == 1)
					{
						redo = 0;
						locX = -3.1 + (rand.NextDouble() * (8.8 + 3.1));
						locY = -2.1 + (rand.NextDouble() * (2.6 + 2.1));

						if (allObjects != null)
						{
							foreach (Vector2 v in allObjects)
							{
								if (v.DistanceTo(new Vector2(locX, locY)) < 1)
								{
									redo = 1;
								}
							}
						}
					}
					fakeObjectList.Add(new FakeObjectRenderer("triangle string", new PointF((float)locX, (float)locY), " ", Color.DarkSlateGray));
					allObjects.Add(new Vector2(locX, locY));
				}


				for (int i = 0; i < numImportants; i++)
				{
					int redo = 1;
					locX = new double();
					locY = new double();

					while (redo == 1)
					{
						redo = 0;
						locX = -3.1 + (rand.NextDouble() * (8.5 + 3.1));
						locY = -2.1 + (rand.NextDouble() * (2.6 + 2.1));

						if (allObjects != null)
						{
							foreach (Vector2 v in allObjects)
							{
								if (v.DistanceTo(new Vector2(locX, locY)) < 1)
								{
									redo = 1;
								}
							}
						}
					}
					fakeObjectList.Add(new FakeObjectRenderer("important string", new PointF((float)locX, (float)locY), " ", Color.DarkSlateGray));
					allObjects.Add(new Vector2(locX, locY));
				}




				for (int i = 0; i < numArrows; i++)
				{
					int redo = 1;
					locX = new double();
					locY = new double();

					while (redo == 1)
					{
						redo = 0;
						locX = -3.1 + (rand.NextDouble() * (8.8 + 3.1));
						locY = -2.1 + (rand.NextDouble() * (2.6 + 2.1));

						if (allObjects != null)
						{
							foreach (Vector2 v in allObjects)
							{
								if (v.DistanceTo(new Vector2(locX, locY)) < 1)
								{
									redo = 1;
								}
							}
						}
					}
					allObjects.Add(new Vector2(locX, locY));

					double angle = rand.NextDouble() * Math.PI * 2;
					double dd = 1 + rand.NextDouble();
					fakeObjectList.Add(new FakeObjectRenderer("arrow string", new PointF((float)locX, (float)locY), " ", colorList2[i], angle, dd));

					//####################################
				}


				if (numPaths > 0)
				{
					for (int i = 0; i < numWalls; i++)
					{
						int redo = 1;
						locX = new double();
						locY = new double();

						while (redo == 1)
						{
							redo = 0;
							locX = -3.1 + (rand.NextDouble() * (8.8 + 3.1));
							locY = -2.1 + (rand.NextDouble() * (2.6 + 2.1));

							if (allObjects != null)
							{
								foreach (Vector2 v in allObjects)
								{
									if (v.DistanceTo(new Vector2(locX, locY)) < 1)
									{
										redo = 1;
									}
								}
							}
						}
						allObjects.Add(new Vector2(locX, locY));
						redo = 1;
						double locX2 = new double();
						double locY2 = new double();

						while (redo == 1)
						{
							redo = 0;
							locX2 = -3.1 + (rand.NextDouble() * (8.8 + 3.1));
							locY2 = -2.1 + (rand.NextDouble() * (2.6 + 2.1));

							if (allObjects != null)
							{
								foreach (Vector2 v in allObjects)
								{
									if (v.DistanceTo(new Vector2(locX2, locY2)) < 1)
									{
										redo = 1;
									}
								}
							}
						}
						allObjects.Add(new Vector2(locX2, locY2));
						wallPoints.Add(new List<PointF>(2) { new PointF((float)locX, (float)locY), new PointF((float)locX2, (float)locY2) });
					}
					for (int i = 0; i < numCircles; i++)
					{
						int redo = 1;
						locX = new double();
						locY = new double();

						while (redo == 1)
						{
							redo = 0;
							locX = -3.1 + (rand.NextDouble() * (8.8 + 3.1));
							locY = -2.1 + (rand.NextDouble() * (2.6 + 2.1));

							if (allObjects != null)
							{
								foreach (Vector2 v in allObjects)
								{
									if (v.DistanceTo(new Vector2(locX, locY)) < 1)
									{
										redo = 1;
									}
								}
							}
						}
						allObjects.Add(new Vector2(locX, locY));
						fakeObjectList.Add(new FakeObjectRenderer("circle string", new PointF((float)locX, (float)locY), " ", Color.DarkSlateGray));
					}
					for (int i = 0; i < numPaths; i++)
					{
						int redo = 1;
						locX = new double();
						locY = new double();

						while (redo == 1)
						{
							redo = 0;
							locX = -3.1 + (rand.NextDouble() * (8.8 + 3.1));
							locY = -2.1 + (rand.NextDouble() * (2.6 + 2.1));

							if (allObjects != null)
							{
								foreach (Vector2 v in allObjects)
								{
									if (v.DistanceTo(new Vector2(locX, locY)) < 1)
									{
										redo = 1;
									}
								}
							}
						}
						allObjects.Add(new Vector2(locX, locY));
						redo = 1;
						double locX2 = new double();
						double locY2 = new double();

						while (redo == 1)
						{
							redo = 0;
							locX2 = -3.1 + (rand.NextDouble() * (8.8 + 3.1));
							locY2 = -2.1 + (rand.NextDouble() * (2.6 + 2.1));

							if (allObjects != null)
							{
								foreach (Vector2 v in allObjects)
								{
									if (v.DistanceTo(new Vector2(locX2, locY2)) < 1)
									{
										redo = 1;
									}
								}
							}
						}
						allObjects.Add(new Vector2(locX2, locY2));


						fakeObjectList.Add(new FakeObjectRenderer("path start string", new PointF((float)locX, (float)locY), " ", colorList[i]));
						fakeObjectList.Add(new FakeObjectRenderer("path end string", new PointF((float)locX2, (float)locY2), " ", colorList[i]));


						//fakeObjectList.Add(new FakeObjectRenderer("x", new PointF((float)locX2, (float)locY2), " ", colorList[i]));
					}

					for (int i = 0; i < numWaypoints; i++)
					{
						int redo = 1;
						locX = new double();
						locY = new double();

						while (redo == 1)
						{
							redo = 0;
							locX = -3.1 + (rand.NextDouble() * (8.8 + 3.1));
							locY = -2.1 + (rand.NextDouble() * (2.6 + 2.1));

							if (allObjects != null)
							{
								foreach (Vector2 v in allObjects)
								{
									if (v.DistanceTo(new Vector2(locX, locY)) < 1)
									{
										redo = 1;
									}
								}
							}
						}
						fakeObjectList.Add(new FakeObjectRenderer("x string", new PointF((float)locX, (float)locY), " ", Color.DarkSlateGray));
						allObjects.Add(new Vector2(locX, locY));
					}
				}
			}
		}

		public List<FakeObjectRenderer> GetObjectList
		{
			get { return fakeObjectList; }
		}


		#region IRender Members

		public string GetName()
		{
			return this.name;
		}

		public void Draw(Renderer r)
		{
			if (wallPoints.Count > 0)
			{
				for (int i = 0; i < wallPoints.Count; i++)
				{
					List<PointF> wallSegment = wallPoints[i];

					for (int j = 0; j < wallSegment.Count - 1; j++)
					{
						GLUtility.DrawLine(new GLPen(color, 5), wallSegment[j], wallSegment[j + 1]);
					}
				}
			}
			if (homeBase.Count > 0)
			{
				GLUtility.DrawLine(new GLPen(color, 1), homeBase[0], homeBase[1]);
				GLUtility.DrawLine(new GLPen(color, 1), homeBase[1], homeBase[2]);
				GLUtility.DrawLine(new GLPen(color, 1), homeBase[2], homeBase[3]);
				GLUtility.DrawString("HOME BASE", Color.DarkSlateGray, new PointF(7.6f, -2.4f));
				GLUtility.DrawString("HOME BASE", Color.DarkSlateGray, new PointF(8.7f, -0.6f));

			}
		}


		public void ClearBuffer()
		{
		}

		public bool VehicleRelative
		{
			get { return false; }
		}

		public int? VehicleRelativeID
		{
			get { return null; }
		}
		#endregion
	}

	//public class AddObjectsEventArgs : EventArgs
	//{
	//    FakeObjectRenderer o;

	//    public FakeObjectRenderer Obstacle
	//    {
	//        get { return o; }
	//    }

	//    public AddObjectsEventArgs(FakeObjectRenderer obj)
	//    {
	//        o = obj;
	//    }
	//}
}
