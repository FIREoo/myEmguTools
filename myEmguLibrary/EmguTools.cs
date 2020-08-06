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
using System.Windows.Media.Imaging;
using System.Windows;
using Point = System.Drawing.Point;

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
#if UNSAFE
        public static Mat cutImage(this Mat src, Rectangle rect)
        {
            if (src.Depth != DepthType.Cv8U)
                throw new Exception("DepthType.Cv8U only!");

            int ch = src.NumberOfChannels;

            Mat rtn = new Mat(rect.Height, rect.Width, DepthType.Cv8U, ch);
            unsafe
            {
                byte* intPtr_src = (byte*)src.DataPointer;
                byte* intPtr_rtn = (byte*)rtn.DataPointer;

                for (int y = 0; y < rect.Height; y++)
                    for (int x = 0; x < rect.Width; x++)
                        for (int c = 0; c < ch; c++)
                        {
                            intPtr_rtn[(y * rect.Width * ch) + (x * ch) + c] = intPtr_src[((y + rect.Y) * src.Width * ch) + ((x + rect.X) * ch) + c];
                        }
            }
            return rtn;
        }
#else
            public static Mat cutImage(this Mat img, Rectangle rect)
		{
			Image<Bgr, Byte> buffer_im = img.ToImage<Bgr, Byte>();
			buffer_im.ROI = rect;
			Image<Bgr, Byte> cropped_im = buffer_im.Copy();
			return cropped_im.Mat;
		}
#endif
        //-----get value-----//
#if UNSAFE
        public static T GetValue<T>(Mat mat, int row, int col)
		{
			unsafe
			{
				if (mat.NumberOfChannels == 1)
					switch (Type.GetTypeCode(typeof(T)))
					{
						case TypeCode.Byte:
							byte* pixelPtr_byte = (byte*)mat.DataPointer;
							return (T)Convert.ChangeType(pixelPtr_byte[row * mat.Cols + col], typeof(T));
						case TypeCode.Int16:
							int* pixelPtr_int = (int*)mat.DataPointer;
							return (T)Convert.ChangeType(pixelPtr_int[row * mat.Cols + col], typeof(T));
						case TypeCode.Double:
							double* pixelPtr_double = (double*)mat.DataPointer;
							return (T)Convert.ChangeType(pixelPtr_double[row * mat.Cols + col], typeof(T));
						default:
							throw new System.ArgumentException("type error", "byte int double");
					}
				else if(mat.NumberOfChannels == 3)
                {//<T>不知道怎麼用
                    MCvScalar _return = new MCvScalar();
                    byte* pixelPtr = (byte*)mat.DataPointer;
                    _return.V0 = pixelPtr[(row * mat.Cols * 3) + (col * 3) + 0];
                    _return.V1 = pixelPtr[(row * mat.Cols * 3) + (col * 3) + 1];
                    _return.V2 = pixelPtr[(row * mat.Cols * 3) + (col * 3) + 2];
                    return (T)Convert.ChangeType(_return, typeof(T)); 
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

			return new MCvScalar(H, S, V);
		}
        //image convert //WPF
        public static BitmapSource MatToBitmap(IInputArray mat)
        {
            WriteableBitmap bitmap = null;
            if (((Mat)mat).NumberOfChannels == 4)
                bitmap = new WriteableBitmap(((Mat)mat).Width, ((Mat)mat).Height, 96.0, 96.0, System.Windows.Media.PixelFormats.Bgra32, null);
            else if (((Mat)mat).NumberOfChannels == 3)
                bitmap = new WriteableBitmap(((Mat)mat).Width, ((Mat)mat).Height, 96.0, 96.0, System.Windows.Media.PixelFormats.Bgr24, null);
            else if (((Mat)mat).NumberOfChannels == 1)
                bitmap = new WriteableBitmap(((Mat)mat).Width, ((Mat)mat).Height, 96.0, 96.0, System.Windows.Media.PixelFormats.Gray8, null);
   

            bitmap.Lock();
            unsafe
            {
                var region = new Int32Rect(0, 0, ((Mat)mat).Width, ((Mat)mat).Height);
                int ch = ((Mat)mat).NumberOfChannels;
                int stride = (((Mat)mat).Width * ch);
                int bitPerPixCh = 8;
                bitmap.WritePixels(region, ((Mat)mat).DataPointer, (stride * ((Mat)mat).Height), stride);
                bitmap.AddDirtyRect(region);
            }
            bitmap.Unlock();

            return bitmap;
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
        /*
        public static class DefaultColor
        {
            public static SolidColorBrush DarkRed = new SolidColorBrush(Color.FromRgb(140, 68, 64));
            public static SolidColorBrush Red = new SolidColorBrush(Color.FromRgb(174, 83, 80));
            public static SolidColorBrush LightRed = new SolidColorBrush(Color.FromRgb(197, 129, 126));
            public static SolidColorBrush DarkGreen = new SolidColorBrush(Color.FromRgb(79, 120, 67));
            public static SolidColorBrush Green = new SolidColorBrush(Color.FromRgb(105, 159, 89));
            public static SolidColorBrush LightGreen = new SolidColorBrush(Color.FromRgb(177, 206, 168));
            public static SolidColorBrush DarkBlue = new SolidColorBrush(Color.FromRgb(59, 78, 169));
            public static SolidColorBrush Blue = new SolidColorBrush(Color.FromRgb(91, 102, 189));
            public static SolidColorBrush LightBlue = new SolidColorBrush(Color.FromRgb(153, 161, 211));
            public static SolidColorBrush DarkYellow = new SolidColorBrush(Color.FromRgb(230, 159, 13));
            public static SolidColorBrush Yellow = new SolidColorBrush(Color.FromRgb(243, 180, 48));
            public static SolidColorBrush LightYellow = new SolidColorBrush(Color.FromRgb(248, 205, 116));

        }
        */
    }
}
