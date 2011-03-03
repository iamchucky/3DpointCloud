using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using Magic.Common.Mapack;


namespace Magic.Common
{
	public class PoseInterpolator
	{
		int bufferSize;
		List<PoseFilterState> stateBuffer;
		public List<PoseFilterState> StateBuffer
		{
			get { return stateBuffer; }
		}
		double oldestTime;
		public double OldestTime
		{
			get { return oldestTime; }
		}
		double newestTime;
		public double NewestTime
		{
			get { return newestTime; }
		}

		public PoseInterpolator(int bufferLength)
		{
			stateBuffer = new List<PoseFilterState>(bufferLength);
			bufferSize = bufferLength;
			newestTime = 0;
			oldestTime = 0;
		}

		public void Add(PoseFilterState state)
		{
			lock (this)
			{
				//if the buffer fills up knock of the last one
				if (stateBuffer.Count >= bufferSize)
				{
					stateBuffer.RemoveAt(0);
					oldestTime = stateBuffer[0].timestamp;
				}

				if (stateBuffer.Count == 0)
				{
					stateBuffer.Add(state);
					oldestTime = stateBuffer[0].timestamp;
					newestTime = stateBuffer[0].timestamp;
				}
				else
				{
					int index = 0;
					for (int i = stateBuffer.Count - 1; i >= 0; i--)
					{
						if (state.timestamp > stateBuffer[i].timestamp)
						{
							index = i + 1;
							break;
						}
					}
					if (index == stateBuffer.Count)
						newestTime = state.timestamp;
					if (index == 0)
						oldestTime = state.timestamp;
					stateBuffer.Insert(index, state);
				}
			}
		}

		public PoseFilterState PoseAtTime(double timestamp)
		{
            if (timestamp < oldestTime || timestamp > newestTime)
            {
                throw new ArgumentOutOfRangeException("Requested time is outside range of buffer");
            }
			PoseFilterState q0;
			PoseFilterState q1;
			//Find the two pose states whose time stamps lie on either side of the desired time
			lock (this)
			{
				PoseFilterState tempState = new PoseFilterState(0, 0, 0, 0, 0, 0, timestamp);
				int index = stateBuffer.BinarySearch(tempState);
				if (index < 0)
				{
					index = ~index;
					q0 = stateBuffer[index - 1];
					q1 = stateBuffer[index];
				}
				else
				{
					return stateBuffer[index];
				}
			}
			return QuaternionInterpolant(q0, q1, timestamp);
		}

		private PoseFilterState QuaternionInterpolant(PoseFilterState q0, PoseFilterState q1, double timestamp)
		{
			double a1;
			double a2;
			double u = (timestamp - q0.timestamp) / (q1.timestamp - q0.timestamp);
			double q01 = q0.q1; double q02 = q0.q2; double q03 = q0.q3; double q04 = q0.q4;
			double q11 = q1.q1; double q12 = q1.q2; double q13 = q1.q3; double q14 = q1.q4;
			//double qSquareSum = q01 * q01 + q02 * q02 + q03 * q03 + q04 * q04;

			//double qp1 = (-1 * q01 * q14 - q02 * q13 + q03 * q12 + q04 * q11) / qSquareSum;
			//double qp2 = (q01 * q13 - q02 * q14 - q03 * q11 + q04 * q12) / qSquareSum;
			//double qp3 = (-1 * q01 * q12 + q02 * q11 - q03 * q14 + q04 * q13) / qSquareSum;
			//double qp4 = (q01 * q11 + q02 * q12 + q03 * q13 + q04 * q14) / qSquareSum;

			//double omega = Math.Acos(qp4);
			//qp1 = qp1 * Math.Sin(u * omega);
			//qp2 = qp2 * Math.Sin(u * omega);
			//qp3 = qp3 * Math.Sin(u * omega);
			//qp4 = Math.Cos(u * omega);

			//double qu1 = (-1 * q01 * qp4 - q02 * qp3 + q03 * qp2 + q04 * qp1);
			//double qu2 = (q01 * qp3 - q02 * qp4 - q03 * qp1 + q04 * qp2);
			//double qu3 = (-1 * q01 * qp2 + q02 * qp1 - q03 * qp4 + q04 * qp3);
			//double qu4 = (q01 * qp1 + q02 * qp2 + q03 * qp3 + q04 * qp4);

			double theta = Math.Acos(q01 * q11 + q02 * q12 + q03 * q13 + q04 * q14);
			
			//Correct for possible loss of numerical precision on q1,q2
			if (Double.IsNaN(theta) || theta == 0)
			{
				theta = 0;
				a1 = 1-u;
				a2 = u;
			}
			else
			{

				a1 = Math.Sin((1 - u) * theta) / Math.Sin(theta);
				a2 = Math.Sin(u * theta) / Math.Sin(theta);
			}


			double qu1 = a1 * q01 + a2 * q11;
			double qu2 = a1 * q02 + a2 * q12;
			double qu3 = a1 * q03 + a2 * q13;
			double qu4 = a1 * q04 + a2 * q14;

			if (Double.IsNaN(qu1) || Double.IsNaN(qu2) || Double.IsNaN(qu3) || Double.IsNaN(qu4))
				throw	new	ArithmeticException("Bad");

			if (Math.Abs(qu1) > 1 || Math.Abs(qu2) > 1 || Math.Abs(qu3) > 1 || Math.Abs(qu4) > 1)
				throw new ArithmeticException("Bigger than 1");

			double xu = u * q1.x + (1 - u) * q0.x;
			double yu = u * q1.y + (1 - u) * q0.y;
			double zu = u * q1.z + (1 - u) * q0.z;

			Matrix P = q0.Covariance * (1 - u) + q1.Covariance * u;
            
            PoseFilterState poseToReturn = new PoseFilterState(xu, yu, zu, qu1, qu2, qu3, qu4, timestamp, P);
            poseToReturn.vx = u * q1.vx + (1 - u) * q0.vx;
            poseToReturn.vy = u * q1.vy + (1 - u) * q0.vy;
            poseToReturn.vz = u * q1.vz + (1 - u) * q0.vz;
            poseToReturn.wx = u * q1.wx + (1 - u) * q0.wx;
            poseToReturn.wy = u * q1.wy + (1 - u) * q0.wy;
            poseToReturn.wz = u * q1.wz + (1 - u) * q0.wz;

            return poseToReturn;
		}
	}
}
