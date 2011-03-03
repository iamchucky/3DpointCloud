using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common
{
	[Serializable]
	public struct RThetaCoordinate
	{
		public float R; public float theta; public float thetaDeg;

		public RThetaCoordinate(float R, float theta)
		{
			this.R = R; this.theta = theta; thetaDeg = theta * 180.0f / (float)Math.PI;
		}

		public RThetaCoordinate(float R, float theta, float thetaDeg)
		{
			this.R = R; this.theta = theta; this.thetaDeg = thetaDeg;
		}

		//Convert to XY vector
		public Vector2 ToVector2()
		{
			return new Vector2(R * Math.Cos(theta), R * Math.Sin(theta));
			//return new Vector2(-R * Math.Sin(theta), R * Math.Cos(theta));
		}

        public Vector2 ToVector2(bool reverseTheta)
        {
            if (reverseTheta)
                return new Vector2(R * Math.Cos(-theta), R * Math.Sin(-theta));
            else
                return new Vector2(R * Math.Cos(theta), R * Math.Sin(theta));
        }

		//Convert to XYZ vector, assume Z is zero
		public Vector3 ToVector3()
		{
			return new Vector3(R * Math.Cos(theta), R * Math.Sin(theta), 0);
		}

		//Convert to XYZ1 vector, assume Z is zero
		public Vector4 ToVector4()
		{
			return new Vector4(R * Math.Cos(theta), R * Math.Sin(theta), 0, 1);
		}

	}

}
