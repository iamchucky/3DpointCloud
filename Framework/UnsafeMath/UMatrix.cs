using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using Magic.Common.Mapack;

namespace Magic.UnsafeMath
{
	/// <summary>Matrix provides the fundamental operations of numerical linear algebra, but is faster becuase it uses unsafe code.</summary>

	[Serializable]
	unsafe public class UMatrix
	{
		private double[] data;
		//note, data is stored like this:
		// [ a b c    is [ a b c d e f g h i]
		//   d e f
		//   g h i ]
		//THIS IS ROW MAJOR

		private int numRows;
		private int numCols;

		/// <summary>Constructs an empty matrix of the given size.</summary>
		/// <param name="rows">Number of rows.</param>
		/// <param name="columns">Number of columns.</param>
		public UMatrix(int rows, int columns)
		{
			this.numRows = rows;
			this.numCols = columns;
			this.data = new double[rows * columns];
		}

        public UMatrix(UMatrix m)
        {
            this.numRows = m.numRows;
            this.numCols = m.numCols;
            this.data = new double[numRows * numCols];
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    this[i, j] = m[i, j];
                }
            }
        }

        public Matrix ToMatrix()
		{
			Matrix toReturn = new Matrix(this.numRows, this.numCols);
			for(int i = 0; i < numRows; i++)
			{
				for(int j = 0; j < numCols; j++)
				{
					toReturn[i, j] = this[j, i];
				}
			}
			return toReturn;
		}

		/// <summary>Constructs a matrix of the given size and assigns a given value to all diagonal elements.</summary>
		/// <param name="rows">Number of rows.</param>
		/// <param name="columns">Number of columns.</param>
		/// <param name="value">Value to assign to the diagnoal elements.</param>
		public UMatrix(int rows, int columns, double value)
			: this(rows, columns)
		{
			int k = Math.Min(rows, columns);
			fixed (double* ptr = data)
			{
				for (int i = 0; i < k; i++)
					(*(ptr + i + (columns * i))) = value;
			}
		}

		public UMatrix(int rows, int columns, double[] value)
			: this(rows, columns)
		{
			int length = rows * columns;

			fixed (double* ptrThis = data, ptrVal = value)
			{
				for (int i = 0; i < length; i++)
				{
					*(ptrThis + i) = *(ptrVal + i);
				}
			}
		}


		/// <summary>Access the value at the given location.</summary>
		public double this[int row, int column]
		{
			set
			{
				if (row >= numRows)
				{
					throw new Exception();
				}
				if (column >= numCols)
				{
					throw new Exception();
				}
				//data[column + (columns * row)] = value; 
				fixed (double* ptr = data)
				{
					(*(ptr + column + (numCols * row))) = value;
				}
			}

			get
			{
				//return data[column + (numCols * row)];
				fixed (double* ptr = data)
				{
					return (*(ptr + column + (numCols * row)));
				}
			}
		}

		/// <summary>Returns a sub matrix extracted from the current matrix.</summary>
		/// <param name="startRow">Start row index</param>
		/// <param name="endRow">End row index</param>
		/// <param name="startColumn">Start column index</param>
		/// <param name="endColumn">End column index</param>
		public UMatrix Submatrix(int startRow, int endRow, int startColumn, int endColumn)
		{
			if ((startRow > endRow) || (startColumn > endColumn) || (startRow < 0) || (startRow >= this.numRows) || (endRow < 0) || (endRow >= this.numRows) || (startColumn < 0) || (startColumn >= this.numCols) || (endColumn < 0) || (endColumn >= this.numCols))
			{
				throw new ArgumentException("Argument out of range.");
			}

			UMatrix X = new UMatrix(endRow - startRow + 1, endColumn - startColumn + 1);
			fixed (double* ptrX = X.data, ptrThis = data)
			{
				for (int i = startRow; i <= endRow; i++)
				{
					for (int j = startColumn; j <= endColumn; j++)
					{
						//X.data[j + (X.numCols * i)] = data[j + (numCols * i)];
						(*(ptrX + j + (X.numCols * i))) = (*(ptrThis + j + (numCols * i)));
					}
				}
			}

			return X;
		}


		/// <summary>Creates a copy of the matrix.</summary>
		public UMatrix Clone()
		{
			UMatrix X = new UMatrix(numRows, numCols);

			fixed (double* ptrX = X.data, ptrThis = data)
			{
				for (int i = 0; i < numRows; i++)
				{
					for (int j = 0; j < numCols; j++)
					{
						//X.data[j + (numCols * i)] = data[j + (numCols * i)];
						(*(ptrX + j + (numCols * i))) = (*(ptrThis + j + (numCols * i)));
					}
				}
			}
			return X;
		}

		/// <summary>Returns the transposed matrix.</summary>
		public UMatrix Transpose()
		{
			UMatrix X = new UMatrix(numCols, numRows);
			fixed (double* ptrX = X.data, ptrThis = data)
			{
				for (int i = 0; i < numRows; i++)
				{
					for (int j = 0; j < numCols; j++)
					{
						//X.data[i + (numRows * j)] = data[j + (numCols * i)];
						(*(ptrX + i + (numRows * j))) = (*(ptrThis + j + (numCols * i)));
					}
				}
			}

			return X;
		}

		/// <summary>Unary minus.</summary>
		public static UMatrix Negate(UMatrix value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			int rows = value.numRows;
			int columns = value.numCols;
			UMatrix X = new UMatrix(rows, columns);
			fixed (double* ptrX = X.data, ptrThis = value.data)
			{
				for (int i = 0; i < rows; i++)
				{
					for (int j = 0; j < columns; j++)
					{
						//X.data[j + (columns * i)] = -value.data[j + (columns * i)];
						(*(ptrX + j + (columns * i))) = -(*(ptrThis + j + (columns * i)));
					}
				}
			}

			return X;
		}

		/// <summary>Unary minus.</summary>
		public static UMatrix operator -(UMatrix value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			return Negate(value);
		}

		/// <summary>Matrix equality.</summary>
		public static bool operator ==(UMatrix left, UMatrix right)
		{
			return Equals(left, right);
		}

		/// <summary>Matrix inequality.</summary>
		public static bool operator !=(UMatrix left, UMatrix right)
		{
			return !Equals(left, right);
		}

		/// <summary>Matrix addition.</summary>
		public static UMatrix Add(UMatrix left, UMatrix right)
		{
			if (left == null)
			{
				throw new ArgumentNullException("left");
			}

			if (right == null)
			{
				throw new ArgumentNullException("right");
			}

			int rows = left.numRows;
			int columns = left.numCols;

			if ((rows != right.numRows) || (columns != right.numCols))
			{
				throw new ArgumentException("Matrix dimension do not match.");
			}

			UMatrix X = new UMatrix(rows, columns);
			fixed (double* ptrX = X.data, ptrL = left.data, ptrR = right.data)
			{
				for (int i = 0; i < rows; i++)
				{
					for (int j = 0; j < columns; j++)
					{
						//X.data[j + (columns * i)] = left.data[j + (columns * i)] + right.data[j + (columns * i)];
						(*(ptrX + j + (columns * i))) = (*(ptrL + j + (columns * i))) + (*(ptrR + j + (columns * i)));
					}
				}
			}
			return X;
		}

		/// <summary>Matrix addition.</summary>
		public static UMatrix operator +(UMatrix left, UMatrix right)
		{
			return Add(left, right);
		}

		/// <summary>Matrix subtraction.</summary>
		public static UMatrix Subtract(UMatrix left, UMatrix right)
		{
			int rows = left.numRows;
			int columns = left.numCols;

			if ((rows != right.numRows) || (columns != right.numCols))
			{
				throw new ArgumentException("Matrix dimension do not match.");
			}

			UMatrix X = new UMatrix(rows, columns);
			fixed (double* ptrX = X.data, ptrL = left.data, ptrR = right.data)
			{
				for (int i = 0; i < rows; i++)
				{
					for (int j = 0; j < columns; j++)
					{
						//X.data[j + (columns * i)] = left.data[j + (columns * i)] - right.data[j + (columns * i)];
						(*(ptrX + j + (columns * i))) = (*(ptrL + j + (columns * i))) - (*(ptrR + j + (columns * i)));
					}
				}
			}
			return X;
		}

		/// <summary>Matrix subtraction.</summary>
		public static UMatrix operator -(UMatrix left, UMatrix right)
		{
			return Subtract(left, right);
		}

		/// <summary>Matrix-scalar multiplication.</summary>
		public static UMatrix Multiply(UMatrix left, double right)
		{
			int rows = left.numRows;
			int columns = left.numCols;

			UMatrix X = new UMatrix(rows, columns);
			fixed (double* ptrX = X.data, ptrL = left.data)
			{
				for (int i = 0; i < rows; i++)
				{
					for (int j = 0; j < columns; j++)
					{
						//X.data[j + (columns * i)] = left.data[j + (columns * i)] * right;
						(*(ptrX + j + (columns * i))) = ((*(ptrL + j + (columns * i))) * right);
					}
				}
			}
			return X;
		}

		/// <summary>Matrix-scalar multiplication.</summary>
		public static UMatrix operator *(UMatrix left, double right)
		{
			return Multiply(left, right);
		}

		/// <summary>Matrix-matrix multiplication.</summary>
		public static UMatrix Multiply(UMatrix left, UMatrix right)
		{

			if (right.numRows != left.numCols)
			{
				throw new ArgumentException("Matrix dimensions are not valid.");
			}

			int rows = left.numRows;
			int columns = right.numCols;
			int size = left.numCols;
			UMatrix X = new UMatrix(rows, columns);

			fixed (double* ptrX = X.data, ptrL = left.data, ptrR = right.data)
			{
				for (int rl = 0; rl < left.numRows; rl++)
				{
					for (int cr = 0; cr < right.numCols; cr++)
					{
						double sum = 0;

						for (int rr = 0; rr < right.numRows; rr++)
						{
							sum += (*(ptrL + rr + (left.numCols * rl))) * (*(ptrR + cr + (right.numCols * rr)));
							//sum += left.data[rr + (left.numCols * rl)] * right.data[cr + (right.numCols * rr)];
						}
						//X.data[cr + (X.numCols * rl)] = sum;
						(*(ptrX + cr + (X.numCols * rl))) = sum;
					}
				}
			}

			//-----------------------------------------------------

			return X;
		}

		/// <summary>Matrix-matrix multiplication.</summary>
		public static UMatrix operator *(UMatrix left, UMatrix right)
		{
			if (left == null)
			{
				throw new ArgumentNullException("left");
			}

			if (right == null)
			{
				throw new ArgumentNullException("right");
			}

			return Multiply(left, right);
		}

		public UMatrix Inverse2x2
		{
			get
			{
				if (numRows != 2 || numCols != 2) throw new InvalidDataException();

				//double det = (data[0 + (numCols * 0)] *
				//              data[1 + (numCols * 1)])
				//             -
				//             (data[1 + (numCols * 0)] *
				//              data[0 + (numCols * 1)]);
				//UMatrix ret = new UMatrix(2, 2);
				//ret[0, 0] = data[1 + (numCols * 1)] / det;
				//ret[0, 1] = -data[1 + (numCols * 0)] / det;
				//ret[1, 0] = -data[0 + (numCols * 0)] / det;
				//ret[1, 1] = data[0 + (numCols * 0)] / det;
				//return ret;

				fixed (double* ptr = data)
				{
					double det = ((*(ptr + 0 + (numCols * 0))) *
											 (*(ptr + 1 + (numCols * 1))))
											 -
											 ((*(ptr + 1 + (numCols * 0))) *
											 (*(ptr + 0 + (numCols * 1))));
					UMatrix ret = new UMatrix(2, 2);
					ret[0, 0] = (*(ptr + 1 + (numCols * 1))) / det;
					ret[0, 1] = -(*(ptr + 1 + (numCols * 0))) / det;
					ret[1, 0] = -(*(ptr + 0 + (numCols * 0))) / det;
					ret[1, 1] = (*(ptr + 0 + (numCols * 0))) / det;
					return ret;
				}
			}
		}
        
        //---------------------------------------------------------
        /* Matlab version       
        %% forward substitution
        %  use to find Z for L*Z = C, given L and C
        function Z = ForwardSub(L,C)
        Z = zeros(size(C));
        for c=1:size(C,2)
            Z(1,c) = C(1,c)/L(1,1);
            for r=2:size(C,1)
               sum = 0;
               for j=1:(r-1)
                   sum = sum + L(r,j)*Z(j,c);
               end
               Z(r,c) = (C(r,c)-sum)/L(r,r);        
            end
        end
        end
        */
        /// <summary> added by Rahul Rastogi
        /// Forward Substitution function to find Z for L*Z = C, given L and C
        /// </summary>
        /// <param name="L">Lower Triangular Matrix, must be square</param>
        /// <param name="C">Matrix to divide into, must have same number of rows as L</param>
        public static UMatrix ForwardSub(UMatrix L, UMatrix C)
        {
            if (L.numCols != L.numRows || L.numRows != C.numRows)
            {                
				throw new ArgumentException("Matrix dimensions are not valid.");
			}
			int rows = L.numCols;
            int cols = C.numCols;

            UMatrix Z = new UMatrix(rows, cols);
            fixed (double* ptrZ = Z.data, ptrL = L.data, ptrC = C.data)
            {
                for (int c = 0; c < C.numCols; c++)
                {
                    (*(ptrZ + c)) = (*(ptrC + c)) / (*(ptrL));
                    for (int r = 1; r < C.numRows; r++) 
                    {
                        double sum = 0;
                        for (int j = 0; j <= (r - 1); j++)
                        {
                            sum += (*(ptrL + j + (L.numCols * r))) * (*(ptrZ + c + (Z.numCols * j)));
                        }
                        (*(ptrZ + c + (Z.numCols * r))) = (*(ptrC + c + (C.numCols * r)) - sum) / (*(ptrL + r + (L.numCols * r)));
                    }
                }
                return Z;
            }
        }


        //-----------------------------------------------------------------
        /* Matlab version
        %% backward substitution
        % use to find X for U*X = Z, given U and Z
        function X = BackSub(U,Z)
        N = size(U,1);
        X = zeros(N);
        for c=1:size(Z,2)
           X(N,c) = Z(N,c)/U(N,N);
           for r=(N-1):-1:1
                sum = 0;
               for j=(r+1):N
                  sum = sum + U(r,j)*X(j,c);
              end
              X(r,c) = (Z(r,c)-sum)/U(r,r);
            end
        end
        end
        */

        /// <summary> added by Rahul Rastogi
        /// Backward Substitution function to find X for U*X = Z, given U and Z
        /// </summary>
        /// <param name="U">Upper Triangular Matrix, must be square</param>
        /// <param name="Z">Matrix to divide into, must have same number of rows as U</param>
        public static UMatrix BackwardSub(UMatrix U, UMatrix Z)
        {
            if (U.numCols != U.numRows || U.numRows != Z.numRows)
            {                
				throw new ArgumentException("Matrix dimensions are not valid.");
			}

            int rows = U.numCols;
            int cols = Z.numCols;
            int n = (U.numRows-1);
            UMatrix X = new UMatrix(rows, cols);
            fixed (double* ptrX = X.data, ptrU = U.data, ptrZ = Z.data)
            {
                for (int c = 0; c < Z.numCols; c++)
                {
                    (*(ptrX + c + (X.numCols * n))) = (*(ptrZ + c + (Z.numCols * n))) / (*(ptrU + n + (U.numCols * n)));
                    for (int r = (n-1); r >= 0; r--)
                    {
                        double sum = 0;
                        for (int j = (r+1); j <= n; j++)
                        {
                            sum += (*(ptrU + j + (U.numCols * r))) * (*(ptrX + c + (X.numCols * j)));
                        }
                        (*(ptrX + c + (X.numCols * r))) = (*(ptrZ + c + (Z.numCols * r)) - sum) / (*(ptrU + r + (U.numCols * r)));
                    }
                }
                return X;
            }
        }
        //----------------------------------------------------------------------------

		public UMatrix LuDecomposition(out UMatrix l, out UMatrix u)
		{
			double[] lu = new double[data.Length];
			Array.Copy(data, lu, data.Length);

			//double[] luRowI;
			double* luRowIPtr;
			double[] luColJ = new double[numRows];
			double[] lData = new double[numRows * numRows];
			double[] uData = new double[numRows * numRows];

			fixed (double* luPtr = lu, luColJPtr = luColJ, lDataPtr = lData, uDataPtr = uData)
			{
				int pivotSign = 1;
				int[] pivotVector = new int[numRows];
				for (int i = 0; i < numRows; i++)
				{
					pivotVector[i] = i;
				}

				// Outer loop.
				for (int j = 0; j < numCols; j++)
				{
					// Make a copy of the j-th column to localize references.
					for (int i = 0; i < numRows; i++)
					{
						*(luColJPtr + i) = *(luPtr + j + i * numCols);
					}

					// Apply previous transformations.
					for (int i = 0; i < numRows; i++)
					{
						luRowIPtr = luPtr + i * numCols;

						// Most of the time is spent in the following dot product.
						//int kmax = (i > j) ? j : i;
						int kmax = Math.Min(i, j);
						double s = 0.0;
						for (int k = 0; k < kmax; k++)
						{
							s += *(luRowIPtr + k) * *(luColJPtr + k);
						}
						*(luRowIPtr + j) = *(luColJPtr + i) -= s;
					}

					// Find pivot and exchange if necessary.
					int p = j;
					for (int i = j + 1; i < numRows; i++)
					{
						if (Math.Abs(*(luColJPtr + i)) > Math.Abs(*(luColJPtr + p)))
						{
							p = i;
						}
					}

					if (p != j)
					{
						for (int k = 0; k < numCols; k++)
						{
							double t = *(luPtr + k + p * numCols);
							*(luPtr + k + p * numCols) = *(luPtr + k + j * numCols);
							*(luPtr + k + j * numCols) = t;
						}

						int v = pivotVector[p];
						pivotVector[p] = pivotVector[j];
						pivotVector[j] = v;

						pivotSign = -pivotSign;
					}

					// Compute multipliers.
					if (j < numRows & *(luPtr + j + j * numCols) != 0.0)
					{
						for (int i = j + 1; i < numRows; i++)
						{
							*(luPtr + j + i * numCols) /= *(luPtr + j + j * numCols);
						}
					}
				}



				for (int i = 0; i < numRows; i++)
				{
					for (int j = 0; j < numCols; j++)
					{
						*(lDataPtr + j + i * numCols) = (j == i) ? 1 : (i < j) ? 0 : *(luPtr + j + i * numCols);
						*(uDataPtr + j + i * numCols) = (i > j) ? 0 : *(luPtr + j + i * numCols);
					}
				}

				l = new UMatrix(numRows, numCols, lData);
				u = new UMatrix(numRows, numCols, uData);
			}

			return new UMatrix(numRows, numCols, lu);
		}

		public UMatrix Inverse()
		{
			UMatrix upper, lower, c, z;

			LuDecomposition(out lower, out upper);
			c = new UMatrix(numRows, numCols, 1);
			z = UMatrix.ForwardSub(lower, c);

			return UMatrix.BackwardSub(upper, z);
		}

		/// <summary>Returns the matrix in a textual form.</summary>
		public override string ToString()
		{
			using (StringWriter writer = new StringWriter())
			{
				for (int i = 0; i < numRows; i++)
				{
					for (int j = 0; j < numCols; j++)
					{
						writer.Write(this.data[j + (this.numCols*i)] + " ");
					}

					writer.WriteLine();
				}

				return writer.ToString();
			}
		}

		private static double Hypotenuse(double a, double b)
		{
			if (Math.Abs(a) > Math.Abs(b))
			{
				double r = b / a;
				return Math.Abs(a) * Math.Sqrt(1 + r * r);
			}

			if (b != 0)
			{
				double r = a / b;
				return Math.Abs(b) * Math.Sqrt(1 + r * r);
			}

			return 0.0;
		}

		public void Zero()
		{
			for (int i = 0; i < numCols * numRows; i++)
				data[i] = 0;
		}
	}
}

