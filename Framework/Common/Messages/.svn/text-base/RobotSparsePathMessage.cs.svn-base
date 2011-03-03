using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Path;

namespace Magic.Common.Messages
{
    [Serializable]
    public class RobotSparsePathMessage
    {
        public RobotSparsePathMessage(int robotID, IPath path)
        {
            Path = path;
            RobotID = robotID;
        }

        public IPath Path { get; set; }
        public int RobotID { get; set; }
    }
}
