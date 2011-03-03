using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magic.Common.DataTypes
{
	/// <summary>
	/// Provides a map of features, useful for SLAM type algorithms
	/// </summary>
	public interface IFeatureMap2D
	{
		List<IFeature2D> Features { get; }
	}

	/// <summary>
	/// A Line feature 
	/// </summary>
	public interface IFeature2DLine : IFeature2D
	{

	}

	/// <summary>
	/// A basic x,y point feature
	/// </summary>
	public interface IFeature2DPoint : IFeature2D
	{

	}

	/// <summary>
	/// A point with Pose
	/// </summary>
	public interface IFeature2DPose : IFeature2DPoint
	{

	}

	public interface IFeature2D
	{
		
	}
}
