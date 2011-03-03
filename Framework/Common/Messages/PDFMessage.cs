using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.DataTypes;

namespace Magic.Common.Messages
{
    [Serializable]
    public class PDFMessage : IRobotMessage
    {
        IOccupancyGrid2D pdf;

        public PDFMessage(IOccupancyGrid2D pdf)
        {
            this.pdf = pdf;
        }

        public IOccupancyGrid2D Pdf
        {
            get { return pdf; }
        }

		#region IRobotMessage Members

		public int RobotID
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}
}
