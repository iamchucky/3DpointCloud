using System;
using System.Collections.Generic;
using Magic.Common.DataStructures;
namespace Magic.Common
{
	public interface ITreeNode : IAsPoint
	{	
		List<ITreeNode> Children { get; }
		bool IsRoot { get; }
		ITreeNode Parent { get; }
		List<Vector2> SimulationPoints { get; }
	}
}
