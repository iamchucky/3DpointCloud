using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Mapack;


namespace Magic.Common.Messages
{
	[Serializable]
	public class HRIPDFMessage : IRobotMessage
	{
		List<HRIPDF> pdfList;

		public HRIPDFMessage(List<HRIPDF> pdfList)
		{
			this.pdfList = pdfList;
		}
		public List<HRIPDF> PDFList
		{
			get { return pdfList; }
		}
		/*
		Vector2 mean;
		Matrix2 sigma;
		public HRIPDFMessage(Vector2 mean, Matrix2 sigma)
		{
			this.mean = mean;
			this.sigma = sigma;
		}
		public Vector2 Mean 
		{
			get { return mean; }
		}
		public Matrix2 Sigma
		{
			get { return sigma; }
		}//*/

		#region IRobotMessage Members

		public int RobotID
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}
}
