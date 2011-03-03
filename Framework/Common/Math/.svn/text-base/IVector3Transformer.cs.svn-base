using System;
using System.Collections.Generic;
using System.Text;

namespace Magic.Common
{
    public interface IVector3Transformer
    {
        Vector3 TransformPoint(Vector3 c);
        Vector3[] TransformPoints(Vector3[] c);
        Vector3[] TransformPoints(ICollection<Vector3> c);

        void TransformPointsInPlace(Vector3[] c);
        void TransformPointsInPlace(IList<Vector3> c);

    }
}
