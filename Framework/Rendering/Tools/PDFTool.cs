using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Magic.Common;
using Magic.Common.Path;
using Magic.Rendering;
using Magic.Rendering.Renderables;
using Magic.Common.Shapes;
using Magic.Common.Mapack;
using Magic.Common.Messages;

namespace Magic.Rendering
{
	public class PDFTool : IRenderToolWithResult<PDFToolCompletedEventArgs>
	{
		#region initializing variables
		private bool isActive = false;
		private bool tempActive = true;

		private bool isDrawing = false;
		private bool firstpointmode = true;
		private bool secondpointmode = false;
		private bool thirdpointmode = false;
		private bool isGenerating = false;
		private bool isEditing = false;
		public event EventHandler<PDFToolCompletedEventArgs> ToolCompleted;
		public event EventHandler UpdateDefault;

		private PointF centerPoint = new Point();
		private PointF movePoint = new Point();
		float height = new float();
		float height2 = new float();
		float width = new float();
		int sigBound = 1;	// drawing 1-sigma bounds
		double angle1 = new double();
		double angle2 = new double();

		Vector2 mu = new Vector2();
		Vector2 firstPDFPoint = new Vector2();
		Vector2 secondPDFPoint = new Vector2();
		Vector2 thirdPDFPoint = new Vector2();
		Matrix2 sigma = new Matrix2();
		PDFFormRenderer PDFrenderer = new PDFFormRenderer();
		PDFReferencePointRenderer PDFRPrenderer = new PDFReferencePointRenderer();
		PDFReferencePointRenderer movePDFPoint = new PDFReferencePointRenderer();
		PDFReferencePointRenderer ghostPoint = new PDFReferencePointRenderer();

		PDFReferencePointRenderer GhostMovePoint1 = new PDFReferencePointRenderer();
		PDFReferencePointRenderer GhostMovePoint2 = new PDFReferencePointRenderer();
		PDFReferencePointRenderer GhostMovePoint3 = new PDFReferencePointRenderer();
		PDFReferencePointRenderer MoveReferencePoint = new PDFReferencePointRenderer();
		Vector2 MoveDeltaReferencePoint1 = new Vector2();
		Vector2 MoveDeltaReferencePoint2 = new Vector2();
		Vector2 MoveDeltaReferencePoint3 = new Vector2();

		Cursor currentCursor = Cursors.Cross;

		//list of dependencies (other tools that must be simultaneously active)
		List<IRenderTool> dependencies;

		//list of conflicts (other tools that can not be simultaneously active)
		List<IRenderTool> conflicts;

		//list of modes tool can be in
		List<string> modeList;

		//bool arguments for mode
		bool modeNewPDF = true;
		bool modeDeletePDF = false;
		bool modeEditPDF = false;
		bool modeMovePDF = false;
		bool shouldEdit = false;
		bool shouldMove = false;
		bool quickPointMode = false;
		int enumerstop = 0;

		//default mode for tool
		string defaultMode;
		#endregion
		#region default modes and dependencies

        private ToolManager myToolManager;
		public PDFTool(ToolManager tm)
        {
            this.myToolManager = tm;
			dependencies = new List<IRenderTool>();
			// Add dependencies here...

			//list of possible modes
			modeList = new List<string>();
			modeList.Add("Add new PDF peak");
			modeList.Add("Add quick PDF peaks");
			//modeList.Add("Remove PDF peak");
			//modeList.Add("Edit PDF points");
			//modeList.Add("Send PDF peak");

			//tool's default mode
			defaultMode = "Add new PDF peak";

		}

        public void BuildConflicts(List<IRenderTool> parallelTools)
        {
            dependencies = new List<IRenderTool>();
            conflicts = new List<IRenderTool>();
            //conflicts.Add(ToolManager.PDFTool);

            foreach (IRenderTool tool in parallelTools)
            {
                if (tool.GetName().StartsWith("Path") ||
                    tool.GetName().StartsWith("Angle") ||
                    tool.GetName().StartsWith("Sketch"))
                {
                    conflicts.Add(tool);
                }
            }

            //conflicts.Add(ToolManager.PathTool);
            //conflicts.Add(ToolManager.AngleTool);
            //conflicts.Add(ToolManager.SketchTool);
			//conflicts.Add(ToolManager.SelectTool);
		}



		/// <summary>
		/// TempDeactivate and TempReactivate : temporarily deactivate/reactivate a tool without disabling it (use carefully)
		/// </summary>
		public void TempDeactivate()
		{
		}
		public void TempReactivate()
		{
		}

		#endregion
		#region mode management
		public void EnableMode(string modeName)
		{
			modeNewPDF = false;
			modeDeletePDF = false;
			modeEditPDF = false;
			modeMovePDF = false;

			if (modeName.Equals("Add new PDF peak"))
			{
				tempActive = true;
				modeNewPDF = true;
				currentCursor = Cursors.Cross;
				firstpointmode = true;
				secondpointmode = false;
				thirdpointmode = false;
				quickPointMode = false;
			}
			else if (modeName.Equals("Add quick PDF peaks"))
			{
				quickPointMode = true;
				tempActive = true;
				modeNewPDF = true;
				currentCursor = Cursors.Cross;
				//firstpointmode = true;
				//secondpointmode = false;
				//thirdpointmode = false;
			}
			else if (modeName.Equals("Remove PDF peak"))
			{
				tempActive = true;
				modeDeletePDF = true;
				currentCursor = Cursors.NoMove2D;

				isGenerating = false;
				firstpointmode = false;
				secondpointmode = false;
				thirdpointmode = false;

				while ((PDFrenderer.PDFReferencePointList.Count % 3 != 0) & PDFrenderer.PDFReferencePointList.Count > 0)
				{
					PDFrenderer.PDFReferencePointList.Remove(PDFrenderer.PDFReferencePointList[PDFrenderer.PDFReferencePointList.Count - 1]);
				}

				//defaultMode = "Add new PDF peak";
				//UpdateDefault(this, new EventArgs());
			}
			else if (modeName.Equals("Edit PDF points"))
			{
				tempActive = true;
				modeEditPDF = true;
				modeNewPDF = false;

				isGenerating = false;
				firstpointmode = false;
				secondpointmode = false;
				thirdpointmode = false;

				while ((PDFrenderer.PDFReferencePointList.Count % 3 != 0) & PDFrenderer.PDFReferencePointList.Count > 0)
				{
					PDFrenderer.PDFReferencePointList.Remove(PDFrenderer.PDFReferencePointList[PDFrenderer.PDFReferencePointList.Count - 1]);
				}

				currentCursor = Cursors.Cross;
			}
			else if (modeName.Equals("Move PDF peak"))
			{
				tempActive = true;
				modeEditPDF = false;
				modeNewPDF = false;
				modeMovePDF = true;

				isGenerating = false;
				firstpointmode = false;
				secondpointmode = false;
				thirdpointmode = false;

				while ((PDFrenderer.PDFReferencePointList.Count % 3 != 0) & PDFrenderer.PDFReferencePointList.Count > 0)
				{
					PDFrenderer.PDFReferencePointList.Remove(PDFrenderer.PDFReferencePointList[PDFrenderer.PDFReferencePointList.Count - 1]);
				}

				currentCursor = Cursors.Cross;
			}
			else if (modeName.Equals("Send PDF peak"))
			{
				Vector2 prevRP1 = new Vector2();
				Vector2 prevRP2 = new Vector2();
				Vector2 prevRP3 = new Vector2();
				double i = new double();
				modeNewPDF = true;
				List<HRIPDF> list = new List<HRIPDF>();
				foreach (PDFReferencePointRenderer rp in PDFrenderer.PDFReferencePointList)
				{
					Vector2 refPoint = new Vector2(rp.X, rp.Y);//third point
					GLUtility.DrawCircle(new GLPen(Color.Red, 1.0f), refPoint, 0.25f);
					prevRP3 = prevRP2;
					prevRP2 = prevRP1;//first point
					prevRP1.X = rp.X;//second point
					prevRP1.Y = rp.Y;
					i++;
					if (i % 3 == 0 & i > 0)
					{
						Vector2 RPfirst = prevRP3;
						Vector2 RPsecond = prevRP2;
						Vector2 RPthird = prevRP1;
						Vector2 midpoint = new Vector2(RPfirst.X + (RPsecond.X - RPfirst.X) / 2, RPfirst.Y + (RPsecond.Y - RPfirst.Y) / 2);

						double sigmaLength = midpoint.DistanceTo(RPsecond);
						double sigmaHeight = midpoint.DistanceTo(RPthird);
						Vector2 delta = new Vector2(RPsecond.X - RPfirst.X, RPsecond.Y - RPfirst.Y);
						double angle = delta.ToRadians();
						sigma = new Matrix2(sigmaLength * Math.Cos(angle) / sigBound, -sigmaLength * Math.Sin(angle) / sigBound, sigmaHeight * Math.Sin(angle) / sigBound, sigmaHeight * Math.Cos(angle) / sigBound);
						Console.WriteLine(sigmaLength + "," + sigmaHeight);
						Console.WriteLine(sigmaLength * Math.Cos(angle) / sigBound + "," + -sigmaLength * Math.Sin(angle) / sigBound + "," + sigmaHeight * Math.Sin(angle) / sigBound + "," + sigmaHeight * Math.Cos(angle) / sigBound);
						list.Add(new HRIPDF(mu, sigma));
					}
				}
				ToolCompleted(this, new PDFToolCompletedEventArgs(list));
				isGenerating = false;
				PDFrenderer = new PDFFormRenderer();

				modeList.Remove("Remove PDF peak");
				modeList.Remove("Edit PDF points");
				modeList.Remove("Send PDF peak");
				modeList.Remove("Move PDF peak");

				defaultMode = "Add new PDF peak";
				modeNewPDF = true;
				firstpointmode = true;
				secondpointmode = false;
				thirdpointmode = false;
				UpdateDefault(this, new EventArgs());
				isDrawing = false;
			}
			UpdateDefault(this, new EventArgs());
		}
		#endregion
		#region IRenderTool Members

		public void OnMouseDown(Renderer r, MouseEventArgs e)
		{
			bool deletedPDF = false;
			if (modeDeletePDF)
			{
				int enumer = 0;
				int pointtype = 0;
				PDFFormRenderer clonePDFRenderer = new PDFFormRenderer();
				foreach (PDFReferencePointRenderer pdfRP in PDFrenderer.PDFReferencePointList)
				{
					if ((movePoint.X - pdfRP.X < 0.25) && (movePoint.Y - pdfRP.Y < 0.25) &&
						(movePoint.X - pdfRP.X > -0.25) && (movePoint.Y - pdfRP.Y > -0.25))
					{
						deletedPDF = true;
						pointtype = enumer % 3;
						int enumer2 = 0;
						foreach (PDFReferencePointRenderer pdfRP2 in PDFrenderer.PDFReferencePointList)
						{
							if (enumer2 < enumer - pointtype || enumer2 > enumer - pointtype + 2)
							{
								clonePDFRenderer.PDFReferencePointList.Add(pdfRP2);
							}
							enumer2++;
						}
                        myToolManager.SelectTool.TempDeactivate();
						break;
					}
					enumer++;
				}
				if (deletedPDF)
				{
					PDFrenderer = clonePDFRenderer;

					if (PDFrenderer.PDFReferencePointList.Count > 0)
					{
						defaultMode = "Send PDF peak";
						modeNewPDF = true;
						firstpointmode = true;
						secondpointmode = false;
						thirdpointmode = false;
						UpdateDefault(this, new EventArgs());
					}
					else
					{
						modeList.Remove("Remove PDF peak");
						modeList.Remove("Edit PDF points");
						modeList.Remove("Send PDF peak");
						modeList.Remove("Move PDF peak");

						defaultMode = "Add new PDF peak";
						modeNewPDF = true;
						firstpointmode = true;
						secondpointmode = false;
						thirdpointmode = false;
						UpdateDefault(this, new EventArgs());

						isDrawing = false;
					}
				}
			}
			else if (modeEditPDF)
			{
				foreach (PDFReferencePointRenderer pdfRP in PDFrenderer.PDFReferencePointList)
				{
					if ((movePoint.X - pdfRP.X < 0.25) && (movePoint.Y - pdfRP.Y < 0.25) &&
						(movePoint.X - pdfRP.X > -0.25) && (movePoint.Y - pdfRP.Y > -0.25))
					{
						shouldEdit = true;
						movePDFPoint = pdfRP;
						ghostPoint = pdfRP;
						currentCursor = Cursors.Hand;
                        myToolManager.SelectTool.TempDeactivate();
						break;
					}
				}
			}
			else if (modeMovePDF)
			{
				/*
				foreach (PDFReferencePointRenderer pdfRP in PDFrenderer.PDFReferencePointList)
				{
					if ((movePoint.X - pdfRP.X < 0.25) && (movePoint.Y - pdfRP.Y < 0.25) &&
						(movePoint.X - pdfRP.X > -0.25) && (movePoint.Y - pdfRP.Y > -0.25))
					{
						shouldEdit = true;
						movePDFPoint = pdfRP;
						ghostPoint = pdfRP;
						currentCursor = Cursors.Hand;
						ToolManager.SelectTool.TempDeactivate();
						break;
					}
				}*/
				int enumer = 0;
				int pointtype = 0;
				//PDFFormRenderer clonePDFRenderer = new PDFFormRenderer();
				foreach (PDFReferencePointRenderer pdfRP in PDFrenderer.PDFReferencePointList)
				{
					if ((movePoint.X - pdfRP.X < 0.25) && (movePoint.Y - pdfRP.Y < 0.25) &&
						(movePoint.X - pdfRP.X > -0.25) && (movePoint.Y - pdfRP.Y > -0.25))
					{
						shouldMove = true;
						//deletedPDF = true;
						pointtype = enumer % 3;
						int enumer2 = 0;
						MoveReferencePoint = pdfRP;
						foreach (PDFReferencePointRenderer pdfRP2 in PDFrenderer.PDFReferencePointList)
						{
							if (enumer2 == enumer - pointtype)
							{
								GhostMovePoint1 = pdfRP2;
								MoveDeltaReferencePoint1.X = GhostMovePoint1.X - MoveReferencePoint.X;
								MoveDeltaReferencePoint1.Y = GhostMovePoint1.Y - MoveReferencePoint.Y;
							}
							else if (enumer2 == enumer - pointtype + 1)
							{
								GhostMovePoint2 = pdfRP2;
								MoveDeltaReferencePoint2.X = GhostMovePoint2.X - MoveReferencePoint.X;
								MoveDeltaReferencePoint2.Y = GhostMovePoint2.Y - MoveReferencePoint.Y;
							}
							else if (enumer2 == enumer - pointtype + 2)
							{
								GhostMovePoint3 = pdfRP2;
								MoveDeltaReferencePoint3.X = GhostMovePoint3.X - MoveReferencePoint.X;
								MoveDeltaReferencePoint3.Y = GhostMovePoint3.Y - MoveReferencePoint.Y;
							}
							enumer2++;
						}
                        myToolManager.SelectTool.TempDeactivate();
						break;
					}
					enumer++;
				}
			}
			else if (e.Button == MouseButtons.Left && quickPointMode && modeNewPDF)
			{
				if (isGenerating == false && isEditing == false && modeNewPDF)
				{
					isDrawing = true;
					//isGenerating = true;
				}
				double radius = 0.5;
				centerPoint = r.ScreenToWorld(e.Location);
				PDFReferencePointRenderer quickpoint1 = new PDFReferencePointRenderer();
				quickpoint1.X = centerPoint.X - radius;
				quickpoint1.Y = centerPoint.Y;
				PDFrenderer.AddReferencePoint(quickpoint1);
				PDFReferencePointRenderer quickpoint2 = new PDFReferencePointRenderer();
				quickpoint2.X = centerPoint.X + radius;
				quickpoint2.Y = centerPoint.Y;
				PDFrenderer.AddReferencePoint(quickpoint2);
				PDFReferencePointRenderer quickpoint3 = new PDFReferencePointRenderer();
				quickpoint3.X = centerPoint.X;
				quickpoint3.Y = centerPoint.Y + radius;
				PDFrenderer.AddReferencePoint(quickpoint3);
				Console.WriteLine("added quickpoint");

				if (!(modeList.Contains("Remove PDF peak")))
				{
					modeList.Add("Remove PDF peak");
					modeList.Add("Edit PDF points");
					modeList.Add("Send PDF peak");
					modeList.Add("Move PDF peak");
					defaultMode = "Send PDF peak";
					UpdateDefault(this, new EventArgs());
				}
			}
			else if (e.Button == MouseButtons.Left && firstpointmode && modeNewPDF)
			{
				if (isGenerating == false && isEditing == false && modeNewPDF)
				{
					isDrawing = true;
					isGenerating = true;
					centerPoint = r.ScreenToWorld(e.Location);
					movePoint = r.ScreenToWorld(e.Location);
					firstPDFPoint.X = centerPoint.X;
					firstPDFPoint.Y = centerPoint.Y;
				}
				secondpointmode = true;
				firstpointmode = false;
				Console.WriteLine("secondpointmode");
				PDFReferencePointRenderer point1 = new PDFReferencePointRenderer();
				point1.X = firstPDFPoint.X;
				point1.Y = firstPDFPoint.Y;
				PDFrenderer.AddReferencePoint(point1);
			}
			else if (e.Button == MouseButtons.Left && secondpointmode && modeNewPDF)
			{
				thirdpointmode = true;
				secondpointmode = false;
				Console.WriteLine("thirdpointmode");
				PDFReferencePointRenderer point2 = new PDFReferencePointRenderer();
				point2.X = secondPDFPoint.X;
				point2.Y = secondPDFPoint.Y;
				PDFrenderer.AddReferencePoint(point2);
			}
			else if (e.Button == MouseButtons.Left && thirdpointmode && modeNewPDF)
			{
				isGenerating = false;
				firstpointmode = true;
				thirdpointmode = false;
				Console.WriteLine("firstpointmode");
				PDFReferencePointRenderer point3 = new PDFReferencePointRenderer();
				point3.X = thirdPDFPoint.X;
				point3.Y = thirdPDFPoint.Y;
				PDFrenderer.AddReferencePoint(point3);

				if (!(modeList.Contains("Remove PDF peak")))
				{
					modeList.Add("Remove PDF peak");
					modeList.Add("Edit PDF points");
					modeList.Add("Send PDF peak");
					modeList.Add("Move PDF peak");
				}

				defaultMode = "Send PDF peak";
				UpdateDefault(this, new EventArgs());
			}
			else if (e.Button == MouseButtons.Right)
			{
				//isDrawing = false;
			}
		}

		public void OnMouseMove(Renderer r, MouseEventArgs e)
		{
			movePoint = r.ScreenToWorld(e.Location);
			if (shouldMove)
			{
				int enumer = 0;
				//enumerstop = 0;
				PDFReferencePointRenderer newpoint1 = new PDFReferencePointRenderer();
				PDFReferencePointRenderer newpoint2 = new PDFReferencePointRenderer();
				PDFReferencePointRenderer newpoint3 = new PDFReferencePointRenderer();
				PDFFormRenderer clonePDFRenderer = new PDFFormRenderer();
				foreach (PDFReferencePointRenderer pdfRP in PDFrenderer.PDFReferencePointList)
				{

					if (GhostMovePoint1 == pdfRP)
					{
						//enumerstop = enumer;
						newpoint1.X = movePoint.X + MoveDeltaReferencePoint1.X;
						newpoint1.Y = movePoint.Y + MoveDeltaReferencePoint1.Y;
						//ghostPoint = newpoint1;
						clonePDFRenderer.AddReferencePoint(newpoint1);
					}

					else if (GhostMovePoint2 == pdfRP)
					{
						//enumerstop = enumer;
						newpoint2.X = movePoint.X + MoveDeltaReferencePoint2.X;
						newpoint2.Y = movePoint.Y + MoveDeltaReferencePoint2.Y;
						//ghostPoint = newpoint2;
						clonePDFRenderer.AddReferencePoint(newpoint2);
					}
					else if (GhostMovePoint3 == pdfRP)
					{
						newpoint3.X = movePoint.X + MoveDeltaReferencePoint3.X;
						newpoint3.Y = movePoint.Y + MoveDeltaReferencePoint3.Y;
						//ghostPoint = newpoint3;
						clonePDFRenderer.AddReferencePoint(newpoint3);
						enumerstop = enumer;
					}
					else
					{
						clonePDFRenderer.AddReferencePoint(PDFrenderer.PDFReferencePointList[enumer]);
					}
					enumer++;
				}
				int test = enumerstop;
				//clonePDFRenderer.PDFReferencePointList[enumerstop - 2] = newpoint1;
				//clonePDFRenderer.PDFReferencePointList[enumerstop - 1] = newpoint2;
				//clonePDFRenderer.PDFReferencePointList[enumerstop - 0] = newpoint3;

				//if (newpoint1 != null || newpoint2 != null || newpoint3 != null)
				//{
				PDFrenderer = clonePDFRenderer;
				//}
				int test2 = enumerstop;
				//shouldMove = false;

			}
			else if (shouldEdit)
			{
				int enumer = 0;
				int enumerstop = 0;
				int pointtype = 0;
				PDFReferencePointRenderer newpoint = new PDFReferencePointRenderer();
				foreach (PDFReferencePointRenderer pdfRP in PDFrenderer.PDFReferencePointList)
				{
					if (ghostPoint == pdfRP)
					{
						enumerstop = enumer;
						newpoint.X = movePoint.X;
						newpoint.Y = movePoint.Y;
						ghostPoint = newpoint;
						pointtype = enumer % 3;
					}
					enumer++;
				}
				if (newpoint != null)
				{
					Vector2 first = new Vector2(PDFrenderer.PDFReferencePointList[enumerstop - pointtype].X, PDFrenderer.PDFReferencePointList[enumerstop - pointtype].Y);
					Vector2 second = new Vector2(PDFrenderer.PDFReferencePointList[enumerstop - pointtype + 1].X, PDFrenderer.PDFReferencePointList[enumerstop - pointtype + 1].Y);
					Vector2 third = new Vector2(PDFrenderer.PDFReferencePointList[enumerstop - pointtype + 2].X, PDFrenderer.PDFReferencePointList[enumerstop - pointtype + 2].Y);
					Vector2 muHolder = new Vector2(first.X + (second.X - first.X) / 2, first.Y + (second.Y - first.Y) / 2);
					double heightHolder = -muHolder.DistanceTo(third);
					bool test = true;
					if (pointtype == 0)
					{
						first = new Vector2(newpoint.X, newpoint.Y);
						muHolder = new Vector2(first.X + (second.X - first.X) / 2, first.Y + (second.Y - first.Y) / 2);
						Vector2 delta = new Vector2(second.X - first.X, second.Y - first.Y);
						double thirdPointAngle = delta.ToRadians();

						third = new Vector2(muHolder.X + Math.Sin(thirdPointAngle) * heightHolder, muHolder.Y - Math.Cos(thirdPointAngle) * heightHolder);
					}
					else if (pointtype == 1)
					{
						second = new Vector2(newpoint.X, newpoint.Y);
						muHolder = new Vector2(first.X + (second.X - first.X) / 2, first.Y + (second.Y - first.Y) / 2);
						Vector2 delta2 = new Vector2(second.X - first.X, second.Y - first.Y);
						double thirdPointAngle = delta2.ToRadians();

						third = new Vector2(muHolder.X + Math.Sin(thirdPointAngle) * heightHolder, muHolder.Y - Math.Cos(thirdPointAngle) * heightHolder);
					}
					else if (pointtype == 2)
					{
						Vector2 delta = new Vector2(second.X - first.X, second.Y - first.Y);
						double thirdPointAngle = delta.ToRadians();
						Vector2 delta1 = new Vector2(movePoint.X - first.X, movePoint.Y - first.Y);
						angle1 = delta1.ToRadians();
						heightHolder = (float)(Math.Sin(thirdPointAngle - angle1) * firstPDFPoint.DistanceTo(new Vector2(movePoint.X, movePoint.Y)));
						third = new Vector2(muHolder.X + Math.Sin(thirdPointAngle) * heightHolder, muHolder.Y - Math.Cos(thirdPointAngle) * heightHolder);
					}
					PDFReferencePointRenderer firstRP = new PDFReferencePointRenderer();
					firstRP.X = first.X;
					firstRP.Y = first.Y;
					PDFReferencePointRenderer secondRP = new PDFReferencePointRenderer();
					secondRP.X = second.X;
					secondRP.Y = second.Y;
					PDFReferencePointRenderer thirdRP = new PDFReferencePointRenderer();
					thirdRP.X = third.X;
					thirdRP.Y = third.Y;
					PDFrenderer.PDFReferencePointList[enumerstop - pointtype] = firstRP;
					PDFrenderer.PDFReferencePointList[enumerstop - pointtype + 1] = secondRP;
					PDFrenderer.PDFReferencePointList[enumerstop - pointtype + 2] = thirdRP;
					if (pointtype == 0)
					{
						ghostPoint = firstRP;
					}
					else if (pointtype == 1)
					{
						ghostPoint = secondRP;
					}
					else
					{
						ghostPoint = thirdRP;
					}
				}

			}
			else
			{
				if (firstpointmode && isGenerating)
				{
					firstPDFPoint = new Vector2(movePoint.X, movePoint.Y);
					secondPDFPoint = new Vector2(movePoint.X, movePoint.Y);
					height2 = -1;

				}
				else if (secondpointmode && isGenerating)
				{
					secondPDFPoint = new Vector2(movePoint.X, movePoint.Y);
					height2 = -1;
				}
				else if (thirdpointmode && isGenerating)
				{
					Vector2 delta1 = new Vector2(movePoint.X - firstPDFPoint.X, movePoint.Y - firstPDFPoint.Y);
					Vector2 delta2 = new Vector2(secondPDFPoint.X - firstPDFPoint.X, secondPDFPoint.Y - firstPDFPoint.Y);
					angle1 = delta1.ToRadians();
					angle2 = delta2.ToRadians();
					height2 = (float)(Math.Sin(angle2 - angle1) * firstPDFPoint.DistanceTo(new Vector2(movePoint.X, movePoint.Y)));
				}
			}
		}


		public void OnMouseUp(Renderer r, MouseEventArgs e)
		{
			if (shouldEdit)
			{
				shouldEdit = false;
			}
			if (shouldMove)
			{
				shouldMove = false;
			}
		}


		public bool IsActive
		{
			get { return isActive; }
			set { isActive = value; }
		}

		public Cursor Cursor
		{
			get { return currentCursor; }
		}

		#endregion

		#region IRender Members

		public string GetName()
		{
			return "PDF Tool";
		}

		public void Draw(Renderer r)
		{
			if (isDrawing == true)
			{
				double i = 0;
				Vector2 prevRP1 = new Vector2();
				Vector2 prevRP2 = new Vector2();
				Vector2 prevRP3 = new Vector2();
				foreach (PDFReferencePointRenderer rp in PDFrenderer.PDFReferencePointList)
				{
					Vector2 refPoint = new Vector2(rp.X, rp.Y);//third point
					GLUtility.DrawCircle(new GLPen(Color.Red, 1.0f), refPoint, 0.25f);
					prevRP3 = prevRP2;
					prevRP2 = prevRP1;//first point
					prevRP1.X = rp.X;//second point
					prevRP1.Y = rp.Y;
					i++;
					if (i % 3 == 0 & i > 0)
					{
						Vector2 RPfirst = prevRP3;
						Vector2 RPsecond = prevRP2;
						Vector2 RPthird = prevRP1;
						Vector2 midpoint = new Vector2(RPfirst.X + (RPsecond.X - RPfirst.X) / 2, RPfirst.Y + (RPsecond.Y - RPfirst.Y) / 2);
						GLUtility.DrawEllipse(new GLPen(Color.Green, 1.0f), midpoint, RPthird, RPsecond);
					}
				}

				mu = new Vector2(firstPDFPoint.X + (secondPDFPoint.X - firstPDFPoint.X) / 2, firstPDFPoint.Y + (secondPDFPoint.Y - firstPDFPoint.Y) / 2);
				Vector2 delta = new Vector2(secondPDFPoint.X - firstPDFPoint.X, secondPDFPoint.Y - firstPDFPoint.Y);
				double thirdPointAngle = delta.ToRadians();
				thirdPDFPoint = new Vector2(mu.X + Math.Sin(thirdPointAngle) * height2, mu.Y - Math.Cos(thirdPointAngle) * height2);

				if (isGenerating)
				{
					GLUtility.DrawCross(new GLPen(Color.Red, 1.0f), firstPDFPoint, 0.3f);
					GLUtility.DrawCross(new GLPen(Color.Red, 1.0f), mu, 0.3f);
					GLUtility.DrawCross(new GLPen(Color.Red, 1.0f), secondPDFPoint, 0.3f);
					GLUtility.DrawCross(new GLPen(Color.Red, 1.0f), thirdPDFPoint, 0.3f);
					GLUtility.DrawEllipse(new GLPen(Color.Blue, 1.0f), mu, secondPDFPoint, thirdPDFPoint);
				}

			}
		}

		public void ClearBuffer()
		{
			isDrawing = false;

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



		#region IRenderTool Members


		public List<string> ModeList
		{
			get { return modeList; }
		}

		public string DefaultMode
		{
			get
			{
				return defaultMode;
			}
			set
			{
				defaultMode = value;
			}
		}

		#endregion

		/// <summary>
		/// Get tool dependencies & conflicts
		/// </summary>
		#region Dependencies & Conflicts
		public List<IRenderTool> Dependencies
		{
			get { return dependencies; }
		}

		public List<IRenderTool> Conflicts
		{
			get { return conflicts; }
		}
		#endregion
	}

	public class PDFToolCompletedEventArgs : EventArgs
	{
		List<HRIPDF> pdflist;

		public List<HRIPDF> PDFList
		{
			get { return pdflist; }
		}
		public PDFToolCompletedEventArgs(List<HRIPDF> l)
		{
			pdflist = l;
		}
	}
}


