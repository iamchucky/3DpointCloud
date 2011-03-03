using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Tao.OpenGl;
using System.Drawing.Imaging;

namespace Magic.Rendering.Renderables
{
	public class BitMapRenderer : IRender
	{
		private object drawlock = new object();
		
		private int textureGeom;
		private int textureID;

		private Bitmap currentImg;
		public Bitmap CurrentImg
		{
			get { return currentImg; }
			private set { currentImg = value; }
		}

		public BitMapRenderer()
		{
			

			/*
			//Create geometry
			textureGeom = Gl.glGenLists(1);
			Gl.glNewList(textureGeom, Gl.GL_COMPILE);
			Gl.glPushMatrix();
			Gl.glTranslatef(0f, 0f, 0f);
			Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_MODULATE);
			Gl.glEnable(Gl.GL_TEXTURE_2D);
			Gl.glBegin(Gl.GL_QUADS);
			Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex2f(0.0f, 0.0f);
			Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex2f(0.0f, 1.0f);
			Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex2f(1.0f, 1.0f);
			Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex2f(1.0f, 0.0f);
			Gl.glEnd();
			Gl.glPopMatrix();
			Gl.glEndList();*/
		}

		public BitMapRenderer(Bitmap bmp)
			: this()
		{
			SetImg(bmp);
		}

		public BitMapRenderer(string filename)// : this(new Bitmap(filename)) 
		{ GLUtility.LoadNewGLTexture(filename, out textureID); }

		public void SetImg(Bitmap bmp)
		{/*
			lock (drawlock)
			{
				CurrentImg = bmp;
				int s = Convert.ToInt32(Math.Pow(2.0, Convert.ToDouble(Math.Ceiling(Math.Log(Convert.ToDouble(Math.Max(CurrentImg.Height, CurrentImg.Width))) / Math.Log(2.0))))),
					bufferSize = s * s;
				Rectangle rect = new Rectangle(0, 0, CurrentImg.Width, CurrentImg.Height);
				CurrentImg.RotateFlip(RotateFlipType.RotateNoneFlipY);
				BitmapData bitmapData = CurrentImg.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
				//byte[] bgrBuffer = new byte[bufferSize * 3];
				//unsafe{
				//	fixed (byte* pBgrBuffer = 

				textureID = 0;// textures[curTextID];
				//Console.WriteLine("Loaded texture id: " + textures[curTextID]);
				Gl.glBindTexture(Gl.GL_TEXTURE_2D, textureID);

				Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGB8, CurrentImg.Width, CurrentImg.Height, 0, Gl.GL_BGR, Gl.GL_UNSIGNED_BYTE, bitmapData.Scan0);
				Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
				Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);

				CurrentImg.UnlockBits(bitmapData);
				CurrentImg.Dispose();
			}*/
		}

		public void SetImg(string filename) { SetImg(new Bitmap(filename)); }

		#region IRender Members

		public string GetName()
		{
			throw new NotImplementedException();
		}

		public void Draw(Renderer cam)
		{
			/*Gl.glPushMatrix();
			Gl.glTranslatef(0f, 0f, 0.0f);
			Gl.glScalef(50, 50, 0.0f);
			Gl.glRotatef(0f, 0.0f, 0.0f, 1.0f);
			Gl.glBindTexture(Gl.GL_TEXTURE_2D, textureID);
			Gl.glColor4f(1.0f, 1.0f, 1.0f, 1.0f);
			Gl.glCallList(textureGeom);
			Gl.glPopMatrix();*/

			GLUtility.FillTexturedRectangle(textureID, new RectangleF(0f, 0f, 300f, 400f));
		}

		public void ClearBuffer()
		{
			throw new NotImplementedException();
		}

		public bool VehicleRelative
		{
			get { return true; }
		}

		public int? VehicleRelativeID
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}
}
