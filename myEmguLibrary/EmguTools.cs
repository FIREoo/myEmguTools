#define UNSAFE//調用的程式不必進入-unsafe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Runtime.InteropServices;
using System.Diagnostics;//trace
using System.Collections.Generic;

namespace myEmguLibrary
{
	public static class MyInvoke
	{

		public static void setToZero(ref Mat mat)
		{
			byte[] value = new byte[mat.Rows * mat.Cols * mat.ElementSize];
			Marshal.Copy(value, 0, mat.DataPointer, mat.Rows * mat.Cols * mat.ElementSize);
		}
		public static void setToZero(ref Mat[] mat)
		{
			for (int i = 0; i < mat.GetLength(0); i++)
			{
				byte[] value = new byte[mat[i].Rows * mat[i].Cols * mat[i].ElementSize];
				Marshal.Copy(value, 0, mat[i].DataPointer, mat[i].Rows * mat[i].Cols * mat[i].ElementSize);
			}
		}
		public static Mat cutImage(this Mat img, Rectangle rect)
		{
			Image<Bgr, Byte> buffer_im = img.ToImage<Bgr, Byte>();
			buffer_im.ROI = rect;
			Image<Bgr, Byte> cropped_im = buffer_im.Copy();
			return cropped_im.Mat;
		}
		//-----get value-----//
#if UNSAFE
		public static T GetValue<T>(Mat mat, int row, int col)
		{
			unsafe
			{
				if (mat.NumberOfChannels == 1)
					switch (typeof(T).ToString())
					{
						case "byte":
							byte* pixelPtr_byte = (byte*)mat.DataPointer;
							return (T)Convert.ChangeType(pixelPtr_byte[row * mat.Cols + col], typeof(T));
						case "int":
							int* pixelPtr_int = (int*)mat.DataPointer;
							return (T)Convert.ChangeType(pixelPtr_int[row * mat.Cols + col], typeof(T));
						case "double":
							double* pixelPtr_double = (double*)mat.DataPointer;
							return (T)Convert.ChangeType(pixelPtr_double[row * mat.Cols + col], typeof(T));
						default:
							throw new System.ArgumentException("type error", "byte int double");
					}
				else
					throw new System.ArgumentException("NumberOfChannels", "!=1");
			}

		}
		public static MCvScalar GetColorM(Mat mat, int row, int col)
		{
			unsafe
			{
				if (mat.NumberOfChannels == 3)
				{
					MCvScalar _return = new MCvScalar();
					byte* pixelPtr = (byte*)mat.DataPointer;
					_return.V0 = pixelPtr[(row * mat.Cols * 3) + (col * 3) + 0];
					_return.V1 = pixelPtr[(row * mat.Cols * 3) + (col * 3) + 1];
					_return.V2 = pixelPtr[(row * mat.Cols * 3) + (col * 3) + 2];
					return _return;
				}
				else
					throw new System.ArgumentException("NumberOfChannels", "!=3");
			}
			return new MCvScalar();
		}
		public static byte GetValue(Mat mat, int row, int col)
		{
			unsafe
			{
				if (mat.NumberOfChannels == 1)
				{
					if (mat.ElementSize == 1)
					{
						byte* pixelPtr = (byte*)mat.DataPointer;
						return pixelPtr[row * mat.Cols + col];
					}
					else
					{
						Trace.Assert(false, "mat element size is not CV_8U");
						return 0;
					}
				}
				else
					return 0;
			}
		}
		public static int GetValue_int(Mat mat, int row, int col)
		{
			unsafe
			{
				if (mat.NumberOfChannels == 1)
				{
					if (mat.ElementSize == 4)
					{
						int* pixelPtr = (int*)mat.DataPointer;
						return pixelPtr[row * mat.Cols + col];
					}
					else
					{
						Trace.Assert(false, "mat element size is not CV_32S");
						return 0;
					}
				}
				else
					return 0;
			}
		}
		public static double GetValue_64F(Mat mat, int row, int col)
		{
			unsafe
			{
				if (mat.NumberOfChannels == 1)
				{
					if (mat.ElementSize == 8)
					{
						double* pixelPtr = (double*)mat.DataPointer;
						return pixelPtr[(row * mat.Cols) + col];
					}
					else
					{
						Trace.Assert(false, "mat element size is not CV_64F");
						return 0;
					}
				}
				else
					return 0;
			}
		}
		public static byte GetValue(Mat mat, Point P)
		{
			if (mat.NumberOfChannels == 1)
			{
				unsafe
				{
					byte* pixelPtr = (byte*)mat.DataPointer;
					return pixelPtr[P.Y * mat.Cols + P.X];
				}
			}
			else
				return 0;
		}
#else
        public static int GetValue(Mat mat, int row, int col)
        {
            if (mat.NumberOfChannels == 1)
            {
                byte[] value = new byte[1];
                Marshal.Copy(mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, value, 0, 1);
                int V = (int)value[0];
                return V;
            }
            else
                return -1;
        }
        public static int GetValue_double(Mat mat, int row, int col)
        {
            if (mat.NumberOfChannels == 1)
            {
                double[] value = new double[1];
                Marshal.Copy(mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, value, 0, 1);
                int V = (int)value[0];
                return V;
            }
            else
                return -1;
        }
#endif

		public static double Distance(Point P1, Point P2)
		{
			return Math.Sqrt(((P1.X - P2.X) * (P1.X - P2.X)) + ((P1.Y - P2.Y) * (P1.Y - P2.Y)));
		}
		public static bool IsClosed(Point P1, Point P2, int D)
		{
			double distance = Distance(P1, P2);
			if (distance <= D)
				return true;
			else
				return false;
		}

		public static void offline(double CenterPoint_x, double CenterPoint_y, double point_end_x, double point_end_y, double distance, out double X, out double Y)
		{
			//(x-x1)^2 + (y-y1)^2 = r^2
			//y = m(x-x1)+y1
			//x = (r/(m^2+1)^0.5)+x1
			//y = ...

			double x1 = CenterPoint_x;
			double y1 = CenterPoint_y;
			double x2 = point_end_x;
			double y2 = point_end_y;


			if (x2 - x1 > 0)
				X = x1 + (distance / (float)Math.Sqrt(((y2 - y1) / (x2 - x1)) * ((y2 - y1) / (x2 - x1)) + 1));
			else if (x2 - x1 < 0)
				X = x1 - (distance / (float)Math.Sqrt(((y2 - y1) / (x2 - x1)) * ((y2 - y1) / (x2 - x1)) + 1));
			else
				X = x1;

			if (y2 - y1 > 0)
				Y = y1 + (distance / (float)Math.Sqrt((1 / ((y2 - y1) / (x2 - x1))) * (1 / ((y2 - y1) / (x2 - x1))) + 1));
			else if ((y2 - y1 < 0))
				Y = y1 - (distance / (float)Math.Sqrt((1 / ((y2 - y1) / (x2 - x1))) * (1 / ((y2 - y1) / (x2 - x1))) + 1));
			else
				Y = y1;
		}
		public static Point offline(Point CenterPoint, Point point_end, double distance)
		{
			offline(CenterPoint.X, CenterPoint.Y, point_end.X, point_end.Y, distance, out double X, out double Y);
			Point P = new Point();
			P.X = (int)X;
			P.Y = (int)Y;
			return P;
		}
		//drawing
		public static void drawCross(ref Mat img, Point P, int size, MCvScalar Mcolor, int thickness = 1)
		{
			CvInvoke.Line(img, new Point(P.X - (size / 2), P.Y - (size / 2)), new Point(P.X + (size / 2), P.Y + (size / 2)), Mcolor, thickness);
			CvInvoke.Line(img, new Point(P.X - (size / 2), P.Y + (size / 2)), new Point(P.X + (size / 2), P.Y - (size / 2)), Mcolor, thickness);
		}
		//color space
		public static MCvScalar hsv2bgr(MCvScalar hsv)
		{
			double hh, p, q, t, ff, S, V;
			S = hsv.V1 / 255.0;
			V = hsv.V2 / 255.0;
			long i;
			MCvScalar output = new MCvScalar();

			hh = hsv.V0;
			if (hh >= 180.0) hh = 0.0;
			hh /= 30.0;
			i = (long)hh;
			ff = hh - i;
			p = V * (1.0 - S);
			q = V * (1.0 - (S * ff));
			t = V * (1.0 - (S * (1.0 - ff)));

			switch (i)
			{
				case 0:
					output.V2 = V;
					output.V1 = t;
					output.V0 = p;
					break;
				case 1:
					output.V2 = q;
					output.V1 = V;
					output.V0 = p;
					break;
				case 2:
					output.V2 = p;
					output.V1 = V;
					output.V0 = t;
					break;

				case 3:
					output.V2 = p;
					output.V1 = q;
					output.V0 = V;
					break;
				case 4:
					output.V2 = t;
					output.V1 = p;
					output.V0 = V;
					break;
				case 5:
				default:
					output.V2 = V;
					output.V1 = p;
					output.V0 = q;
					break;
			}
			output.V0 *= 255;
			output.V1 *= 255;
			output.V2 *= 255;
			return output;
		}
		public static MCvScalar bgr2hsv(MCvScalar bgr)
		{
			double B = bgr.V0;
			double G = bgr.V1;
			double R = bgr.V2;

			double Max = Math.Max(Math.Max(B, G), R);
			double Min = Math.Min(Math.Min(B, G), R);
			double V = Max;
			double S = 0;
			if (Max == 0)
				S = 0;
			else
				S = 1 - (Min / Max);
			double H = 0;
			if (Max == Min)
				H = 0;
			else if (Max == R && G >= B)
				H = 60 * ((G - B) / (Max - Min)) + 0;
			else if (Max == R && G < B)
				H = 60 * ((G - B) / (Max - Min)) + 360;
			else if (Max == G)
				H = 60 * ((B - R) / (Max - Min)) + 120;
			else if (Max == B)
				H = 60 * ((R - G) / (Max - Min)) + 240;

			H /= 2;
			S *= 255;
			V *= 255;

			return new MCvScalar(H, S, V);
		}

		/// <summary>HSV to BGR</summary>
		public static MCvScalar ToBGR(this MCvScalar input)
		{
			return hsv2bgr(input);
		}
		public static Color toColor(MCvScalar input)
		{
			Color color = Color.FromArgb((int)input.V2, (int)input.V1, (int)input.V0);
			return color;
		}
		//color table
		public static MCvScalar aColorM(int tag)
		{
			switch (tag)
			{
				case 1:
					return new MCvScalar(142, 134, 235);
				case 2:
					return new MCvScalar(176, 205, 30);
				case 3:
					return new MCvScalar(217, 198, 153);
				case 4:
					return new MCvScalar(141, 215, 243);
				case 5:
					return new MCvScalar(203, 176, 1);
				case 6:
					return new MCvScalar(135, 80, 0);
				case 7:
					return new MCvScalar(154, 177, 251);
				case 8:
					return new MCvScalar(82, 80, 156);
				case 9:
					return new MCvScalar(79, 198, 145);
				case 10:
					return new MCvScalar(139, 123, 186);
				case 11:
					return new MCvScalar(169, 159, 161);
				case 12:
					return new MCvScalar(29, 230, 181);
				case 13:
					return new MCvScalar(231, 191, 207);
				case 14:
					return new MCvScalar(164, 73, 163);
				case 15:
					return new MCvScalar(205, 202, 192);
				default:
					return new MCvScalar(0, 0, 0);
			}
		}
	}
}
