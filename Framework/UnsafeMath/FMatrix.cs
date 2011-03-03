using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace Magic.UnsafeMath
{
	/// <summary>Matrix provides the fundamental operations of numerical linear algebra, but is faster becuase it uses unsafe code.</summary>

	[Serializable]
	public class FMatrix
	{
		private double[] data;
		//note, data is stored like this:
		// [ a b c    is [ a b c d e f g h i]
		//   d e f
		//   g h i ]
		//THIS IS ROW MAJOR

		private int rows;
		private int columns;

		/// <summary>Constructs an empty matrix of the given size.</summary>
		/// <param name="rows">Number of rows.</param>
		/// <param name="columns">Number of columns.</param>
		public FMatrix(int rows, int columns)
		{
			this.rows = rows;
			this.columns = columns;
			this.data = new double[rows * columns];
		}

		/// <summary>Constructs a matrix of the given size and assigns a given value to all diagonal elements.</summary>
		/// <param name="rows">Number of rows.</param>
		/// <param name="columns">Number of columns.</param>
		/// <param name="value">Value to assign to the diagnoal elements.</param>
		public FMatrix(int rows, int columns, double value)
			: this(rows, columns)
		{
			int k = Math.Min(rows, columns);
			for (int i = 0; i < k; i++)
				data[i + (i * columns)] = value;
		}

		
		/// <summary>Access the value at the given location.</summary>
		public double this[int row, int column]
		{
			set
			{
				if (row >= rows)
				{
					throw new Exception();
				}
				if (column >= columns)
				{
					throw new Exception();
				}
				data[column + (columns * row)] = value; 
				//fixed (double* ptr = data)
				//{
				//  (*(ptr + column + (columns * row))) = value;
				//}
			}

			get
			{
				return data[column + (columns * row)];
				//fixed (double* ptr = data)
				//{
				//  return (*(ptr + column + (columns * row)));
				//}
			}
		}

		/// <summary>Returns a sub matrix extracted from the current matrix.</summary>
		/// <param name="startRow">Start row index</param>
		/// <param name="endRow">End row index</param>
		/// <param name="startColumn">Start column index</param>
		/// <param name="endColumn">End column index</param>
		public FMatrix Submatrix(int startRow, int endRow, int startColumn, int endColumn)
		{
			if ((startRow > endRow) || (startColumn > endColumn) || (startRow < 0) || (startRow >= this.rows) || (endRow < 0) || (endRow >= this.rows) || (startColumn < 0) || (startColumn >= this.columns) || (endColumn < 0) || (endColumn >= this.columns))
			{
				throw new ArgumentException("Argument out of range.");
			}

			FMatrix X = new FMatrix(endRow - startRow + 1, endColumn - startColumn + 1);
			//fixed (double* ptrX = X.data, ptrThis = data)
			//{
				
				for (int i = startRow; i <= endRow; i++)
				{
					for (int j = startColumn; j <= endColumn; j++)
					{
						X.data[j + (X.columns * i)] = data[j + (columns * i)];
						//(*(ptrX + j + (columns * i))) = (*(ptrThis + j + (columns * i)));
					}
				}
			//}

			return X;
		}


		/// <summary>Creates a copy of the matrix.</summary>
		public FMatrix Clone()
		{
			FMatrix X = new FMatrix(rows, columns);

			//fixed (double* ptrX = X.data, ptrThis = data)
			//{
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < columns; j++)
				{
					X.data[j + (columns * i)] = data[j + (columns * i)];
					//(*(ptrX + j + (columns * i))) = (*(ptrThis + j + (columns * i)));
				}
			}
			//}
			return X;
		}

		/// <summary>Returns the transposed matrix.</summary>
		public FMatrix Transpose()
		{
			FMatrix X = new FMatrix(columns, rows);
			//fixed (double* ptrX = X.data, ptrThis = data)
			//{
				for (int i = 0; i < rows; i++)
				{
					for (int j = 0; j < columns; j++)
					{
						X.data[i + (rows * j)] = data[j + (columns * i)];
				//		(*(ptrX + j + (columns * i))) = (*(ptrThis + i + (columns * j)));
					}
				}
			//}

			return X;
		}


		/// <summary>Unary minus.</summary>
		public static FMatrix Negate(FMatrix value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			int rows = value.rows;
			int columns = value.columns;
			FMatrix X = new FMatrix(rows, columns);
			//fixed (double* ptrX = X.data, ptrThis = value.data)
			//{
				for (int i = 0; i < rows; i++)
				{
					for (int j = 0; j < columns; j++)
					{
						X.data[j + (columns * i)] = -value.data[j + (columns * i)];
						//(*(ptrX + j + (columns * i))) = -(*(ptrThis + j + (columns * i)));
					}
				//}
			}

			return X;
		}

		/// <summary>Unary minus.</summary>
		public static FMatrix operator -(FMatrix value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}

			return Negate(value);
		}

		/// <summary>Matrix equality.</summary>
		public static bool operator ==(FMatrix left, FMatrix right)
		{
			return Equals(left, right);
		}

		/// <summary>Matrix inequality.</summary>
		public static bool operator !=(FMatrix left, FMatrix right)
		{
			return !Equals(left, right);
		}

		/// <summary>Matrix addition.</summary>
		public static FMatrix Add(FMatrix left, FMatrix right)
		{
			if (left == null)
			{
				throw new ArgumentNullException("left");
			}

			if (right == null)
			{
				throw new ArgumentNullException("right");
			}

			int rows = left.rows;
			int columns = left.columns;

			if ((rows != right.rows) || (columns != right.columns))
			{
				throw new ArgumentException("Matrix dimension do not match.");
			}

			FMatrix X = new FMatrix(rows, columns);
			//fixed (double* ptrX = X.data, ptrL = left.data, ptrR = right.data)
			//{
				for (int i = 0; i < rows; i++)
				{
					for (int j = 0; j < columns; j++)
					{
						X.data[j + (columns * i)] = left.data[j + (columns * i)] + right.data[j + (columns * i)];
						//(*(ptrX + j + (columns * i))) = (*(ptrL + j + (columns * i))) + (*(ptrR + j + (columns * i)));
					}
				}
			//}
			return X;
		}

		/// <summary>Matrix addition.</summary>
		public static FMatrix operator +(FMatrix left, FMatrix right)
		{
			return Add(left, right);
		}

		/// <summary>Matrix subtraction.</summary>
		public static FMatrix Subtract(FMatrix left, FMatrix right)
		{
			int rows = left.rows;
			int columns = left.columns;

			if ((rows != right.rows) || (columns != right.columns))
			{
				throw new ArgumentException("Matrix dimension do not match.");
			}

			FMatrix X = new FMatrix(rows, columns);
			//fixed (double* ptrX = X.data, ptrL = left.data, ptrR = right.data)
			//{
				for (int i = 0; i < rows; i++)
				{
					for (int j = 0; j < columns; j++)
					{
						X.data[j + (columns * i)] = left.data[j + (columns * i)] - right.data[j + (columns * i)];
						//(*(ptrX + j + (columns * i))) = (*(ptrL + j + (columns * i))) - (*(ptrR + j + (columns * i)));
					}
				}
			//}
			return X;
		}

		/// <summary>Matrix subtraction.</summary>
		public static FMatrix operator -(FMatrix left, FMatrix right)
		{
			return Subtract(left, right);
		}

		/// <summary>Matrix-scalar multiplication.</summary>
		public static FMatrix Multiply(FMatrix left, double right)
		{
			int rows = left.rows;
			int columns = left.columns;

			FMatrix X = new FMatrix(rows, columns);
			//fixed (double* ptrX = X.data, ptrL = left.data)
			//{
				for (int i = 0; i < rows; i++)
				{
					for (int j = 0; j < columns; j++)
					{
						X.data[j + (columns * i)] = left.data[j + (columns * i)] * right;
						//(*(ptrX + j + (columns * i))) = ((*(ptrL + j + (columns * i))) * right);
					}
				}
			//}
			return X;
		}

		/// <summary>Matrix-scalar multiplication.</summary>
		public static FMatrix operator *(FMatrix left, double right)
		{
			return Multiply(left, right);
		}

		/// <summary>Matrix-matrix multiplication.</summary>
		public static FMatrix Multiply(FMatrix left, FMatrix right)
		{

			if (right.rows != left.columns)
			{
				throw new ArgumentException("Matrix dimensions are not valid.");
			}

			int rows = left.rows;
			int columns = right.columns;
			int size = left.columns;
			FMatrix X = new FMatrix(rows, columns);

			//fixed (double* ptrX = X.data, ptrL = left.data, ptrR = right.data)
			//{
				for (int rl = 0; rl < left.rows; rl++)
				{
					for (int cr = 0; cr < right.columns; cr++)
					{
						double sum = 0;

						for (int rr = 0; rr < right.rows; rr++)
						{
							//sum += (*(ptrL + rr + (columns * rl))) * (*(ptrR + cr + (columns * rr)));
							sum += left.data[rr + (left.columns * rl)] * right.data[cr + (right.columns * rr)];
						}
						X.data[cr + (X.columns * rl)] = sum;
						//(*(ptrX + cr + (columns * rl))) = sum;
					}
				//}
			}

			//-----------------------------------------------------

			return X;
		}

		/// <summary>Matrix-matrix multiplication.</summary>
		public static FMatrix operator *(FMatrix left, FMatrix right)
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


		public FMatrix Inverse2x2
		{
			get
			{
				if (rows != 2 || columns != 2) throw new InvalidDataException();

					double det = (data[0 + (columns * 0)] *
											  data[1 + (columns * 1)])
											 -
											 (data[1 + (columns * 0)] *
											  data[0 + (columns * 1)]);
					FMatrix ret = new FMatrix(2, 2);
					ret[0, 0] = data[ 1 + (columns * 1)] / det;
					ret[0, 1] = -data[1 + (columns * 0)] / det;
					ret[1, 0] = -data[0 + (columns * 0)] / det;
					ret[1, 1] = data[0 + (columns * 0)] / det;
					return ret;
				
				//fixed (double* ptr = data)
				//{
				//  double det = ((*(ptr + 0 + (columns * 0))) *
				//               (*(ptr + 1 + (columns * 1)))) 
				//               -
				//               ((*(ptr + 1 + (columns * 0))) *
				//               (*(ptr + 0 + (columns * 1))));					
				//  FMatrix ret = new FMatrix(2, 2);
				//  ret[0, 0] = (*(ptr + 1 + (columns * 1))) / det;
				//  ret[0, 1] = -(*(ptr + 1 + (columns * 0))) / det;
				//  ret[1, 0] = -(*(ptr + 0 + (columns * 0))) / det;
				//  ret[1, 1] = (*(ptr + 0 + (columns * 0))) / det;
				//  return ret;
				//}
			}
		}
	
		/// <summary>Returns the matrix in a textual form.</summary>
		public override string ToString()
		{
			using (StringWriter writer = new StringWriter())
			{
				for (int i = 0; i < rows; i++)
				{
					for (int j = 0; j < columns; j++)
					{
						//writer.Write(this.data[i][j] + " ");
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
			for (int i = 0; i < columns * rows; i++)
				data[i] = 0;
		}

	}
}
