using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Reflection;
using Magic.Common;

namespace Magic.Rendering
{
	public class SensorCoverage
	{
		public SensorPose location = new SensorPose();
		private Color renderColor = Color.Red;
		double fov = 0; double range = 0;
		public float opacity = .10f;
		string id;
		
		//private PropertyInfo[] optionProperties;

		//public bool ShouldDisplay(SensorCoverageRenderOptions options, string sensorName)
		//{
		//  foreach (PropertyInfo pi in optionProperties)
		//  {
		//    if (pi.Name.Equals("Show" + sensorName))
		//    {
		//      return (bool)pi.GetValue(options, null);
		//    }
		//  }
		//  return false;
		//}

		public SensorCoverage(double fovDegrees, double range, Color color, SensorPose sensor, string name)
		{
			this.fov = fovDegrees * (Math.PI / 180.0);
			this.range = range;
			this.location = sensor;
			this.renderColor = color;
			this.id = name;
			//get the option by name
			//Type coverageOptionsType = typeof(SensorCoverageRenderOptions);
			//optionProperties = coverageOptionsType.GetProperties();
		}

		public SensorCoverage(double fovDegrees, double range, SensorPose location, Color color, string id)
		{
			this.fov = fovDegrees * (Math.PI / 180.0);
			this.range = range;
			this.location = location;
			this.renderColor = color;
			this.id = id;
			//get the option by name
			//Type coverageOptionsType = typeof(SensorCoverageRenderOptions);
			//optionProperties = coverageOptionsType.GetProperties();
		}
		public void Draw()
		{
			GLUtility.GoToTransform((float)location.x, (float)location.y);
			GLUtility.GoToTransform((float)location.yaw);
			GLUtility.FillCone(renderColor, opacity, new PointF(0, 0), (float)range, (float)-fov / 2.0f, (float)fov / 2.0f);
			GLUtility.ComeBackFromTransform();
			GLUtility.ComeBackFromTransform();
		}
	}
}
