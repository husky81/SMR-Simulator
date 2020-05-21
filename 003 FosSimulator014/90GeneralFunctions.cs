using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace bck.SMR_simulator.general_functions
{
    public static class GF
    {
        internal static double Determinant(double[,] mat)
        {
            int size = mat.GetLength(0);
            //int sizeN = mat.GetLength(1);
            //if (size != sizeN) return 0;

            if (size <= 2) return mat[0, 0] * mat[1, 1] - mat[0, 1] * mat[1, 0];
            
            double det = 0;

            for (int i = 0; i < size; i++)
            {
                det += Math.Pow(-1, i) * mat[i, 0] * GF.Determinant(cofactor_ij(mat, i));
            }

            return det;
            double[,] cofactor_ij(double[,] matrix, int m)
            {
                //matrix[m,0] 항이 없는 행렬 생성
                int sizeC = matrix.GetLength(0);
                double[,] cofactorMat = new double[sizeC - 1, sizeC - 1];

                for (int i = 0; i < m; i++)
                {
                    for (int j = 1; j < sizeC; j++)
                    {
                        cofactorMat[i, j - 1] = matrix[i, j];
                    }
                }
                for (int i = m+1; i < sizeC; i++)
                {
                    for (int j = 1; j < sizeC; j++)
                    {
                        cofactorMat[i-1, j - 1] = matrix[i, j];
                    }
                }
                return cofactorMat;
            }
        }

        private static double[,] MatrixCreate(int rows, int cols)
        {
            // allocates/creates a matrix initialized to all 0.0. assume rows and cols > 0
            // do error checking here
            double[,] result = new double[rows, cols];
            return result;
        }
        public static T DeepClone<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }//딥클론인데 순환 객체인경우 용량 무한ㅋㅋ 쓰기 좀 그럼.
        public static double[,] TransformMatrix3_ZYX(double dx, double dy, double dz, double Angle)
        {
            double[,] ZRM = new double[3, 3];
            double[,] YRM = new double[3, 3];
            double[,] XRM = new double[3, 3];
            double[,] XYZRM;
            double Cx, Cy, Cz, L, Cx2, Cy2, Cz2;
            L = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (L == 0.0d)
            {
                return null;
            }

            Cx = dx / L; Cy = dy / L; Cz = dz / L; Cx2 = Cx * Cx; Cy2 = Cy * Cy; Cz2 = Cz * Cz;
            //Z - Direction Rotation Matrix
            ZRM[2, 2] = 1.0d;
            if (Cx2 + Cy2 != 0)
            {
                ZRM[0, 0] = Cx / Math.Sqrt(Cx2 + Cy2);
                ZRM[0, 1] = Cy / Math.Sqrt(Cx2 + Cy2);
                ZRM[1, 0] = -Cy / Math.Sqrt(Cx2 + Cy2);
                ZRM[1, 1] = Cx / Math.Sqrt(Cx2 + Cy2);
            }
            else
            {
                ZRM[0, 0] = 1.0d;
                ZRM[1, 1] = 1.0d;
            }
            //Y - Direction Rotation Matrix
            YRM[1, 1] = 1.0d;
            if (Cx2 + Cz2 != 0)
            {
                YRM[0, 0] = Math.Sqrt(Cx2 + Cy2);
                YRM[0, 2] = Cz;
                YRM[2, 0] = -Cz;
                YRM[2, 2] = Math.Sqrt(Cx2 + Cy2);
            }
            else
            {
                YRM[0, 0] = 1.0d;
                YRM[2, 2] = 1.0d;
            }
            //X - Direction Rotation Matrix
            XRM[0, 0] = 1.0d;
            XRM[1, 1] = Math.Cos(Angle);
            XRM[1, 2] = Math.Sin(Angle);
            XRM[2, 1] = -Math.Sin(Angle);
            XRM[2, 2] = Math.Cos(Angle);

            //XYZ - Direction Rotation Matrix
            XYZRM = Multiply(XRM, Multiply(YRM, ZRM));
            return XYZRM;
        }//좌표변환핸렬
        public static double[,] TransformMatrix3_YZX(double dx, double dy, double dz, double Angle)
        {
            double[,] ZRM = new double[3, 3];
            double[,] YRM = new double[3, 3];
            double[,] XRM = new double[3, 3];
            double[,] XYZRM;

            double Cx, Cy, Cz, L, Cx2, Cy2, Cz2;
            L = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            Cx = dx / L; Cy = dy / L; Cz = dz / L; Cx2 = Cx * Cx; Cy2 = Cy * Cy; Cz2 = Cz * Cz;
            //Y - Direction Rotation Matrix
            YRM[2, 2] = 1.0d;
            if(Cx2 + Cz2 != 0)
            {
                YRM[0, 0] = Cx / Math.Sqrt(Cx2 + Cz2);
                YRM[0, 2] = Cz / Math.Sqrt(Cx2 + Cz2);
                YRM[2, 0] = -Cz / Math.Sqrt(Cx2 + Cz2);
                YRM[2, 2] = Cx / Math.Sqrt(Cx2 + Cz2);
            }
            else
            {
                YRM[0, 0] = 1.0d;
                YRM[2, 2] = 1.0d;
            }
            //Z - Direction Rotation Matrix
            ZRM[2, 2] = 1.0d;
            if(Cx2 + Cy2 != 0)
            {
                ZRM[0, 0] = Math.Sqrt(Cx2 + Cz2);
                ZRM[0, 1] = Cy;
                ZRM[1, 0] = -Cy;
                ZRM[1, 1] = Math.Sqrt(Cx2 + Cz2);
            }
            else
            {
                ZRM[0, 0] = 1.0d;
                ZRM[1, 1] = 1.0d;
            }
            //X - Direction Rotation Matrix
            XRM[0, 0] = 1.0d;
            XRM[1, 1] = Math.Cos(Angle);
            XRM[1, 2] = Math.Sin(Angle);
            XRM[2, 1] = -Math.Sin(Angle);
            XRM[2, 2] = Math.Cos(Angle);

            //XYZ - Direction Rotation Matrix
            XYZRM = GF.Multiply(XRM, GF.Multiply(ZRM, YRM));

            return XYZRM;
        }//좌표변환핸렬
        public static double[,] Multiply(double[,] a, double[,] b)
        {
            int m1 = a.GetLength(0);
            int n1 = a.GetLength(1);
            int m2 = b.GetLength(0);
            int n2 = b.GetLength(1);

            if (!n1.Equals(m2))
            {
                return null;
            }

            double[,] rlt = new double[m1, n2];
            for (int i = 0; i < m1; i++)
            {
                for (int j = 0; j < n2; j++)
                {
                    for (int k = 0; k < n1; k++)
                    {
                        rlt[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
            return rlt;
        }//행렬곱
        public static double[] Multiply(double[,] a, double[] b)
        {
            int m1 = a.GetLength(0);
            int n1 = a.GetLength(1);
            int m2 = b.GetLength(0);

            if (!n1.Equals(m2))
            {
                return null;
            }

            double[] rlt = new double[m1];
            for (int i = 0; i < m1; i++)
            {
                for (int k = 0; k < n1; k++)
                {
                    rlt[i] += a[i, k] * b[k];
                }
            }
            return rlt;
        }
        internal static double[,] Multiply(double[,] a, double n)
        {
            int m1 = a.GetLength(0);
            int n1 = a.GetLength(1);

            double[,] rlt = new double[m1, n1];
            for (int i = 0; i < m1; i++)
            {
                for (int j = 0; j < n1; j++)
                {
                    rlt[i, j] += a[i, j] * n;
                }
            }
            return rlt;
        }
        internal static double[,] MatrixPlus(double[,] a, double[,] b)
        {
            int m1 = a.GetLength(0);
            int n1 = a.GetLength(1);

            double[,] c = new double[m1, n1];
            for (int i = 0; i < m1; i++)
            {
                for (int j = 0; j < n1; j++)
                {
                    c[i, j] = a[i, j] + b[i, j];
                }
            }
            return c;
        }

        internal static double[,] Transpose(double[,] a)
        {
            int m = a.GetLength(0);
            int n = a.GetLength(1);

            double[,] b = new double[n, m];

            for(int i = 0; i < m; i++)
            {
                for(int j = 0; j < n; j++)
                {
                    b[j, i] = a[i, j];
                }
            }
            return b;
        }//대칭행렬
        public static double[,] InverseMatrix_GaussSolver(double[,] matrix)
        {
            int dof = matrix.GetLength(0);
            if (!dof.Equals(matrix.GetLength(1))) return null;

            double[,] a = new double[dof, dof];
            for(int i = 0; i < dof; i++)
            {
                for(int j = 0; j < dof; j++)
                {
                    a[i, j] = matrix[i, j];
                }
            }

            double[,] b = new double[dof, dof];

            //b를 단위행렬로 만들고
            for(int i = 0; i < dof; i++)
            {
                b[i, i] = 1;
            }

            int m2;

            for(int j = 0; j < dof; j++)
            {
                int m = j;
                m2 = FindNonZeroDownSide(m, j);
                if (m2 == -1) continue;
                ChangeM(m, m2);
                Multipl(m, 1 / a[m, j]);

                for (int i = j+1; i < dof; i++)
                {
                    if (a[i, j] != 0)
                    {
                        Multipl(i, 1 / a[i, j]);
                        Substract(i, m);
                    }
                }
            }

            for(int j = dof - 1; j > 0; j--)
            {
                int m = j;
                for(int i = m - 1; i >= 0; i--)
                {
                    if (a[i, j] != 0)
                    {
                        SubstractMultipl(i, m, a[i, j]);
                    }
                }
            }
            


            return b;
            void ChangeM(int mm1, int mm2)
            {
                if (mm1 == mm2) return;
                double tmpD;
                for (int j = 0; j < dof; j++)
                {
                    tmpD = a[mm1, j];
                    a[mm1, j] = a[mm2, j];
                    a[mm2, j] = tmpD;
                }
                for (int j = 0; j < dof; j++)
                {
                    tmpD = b[mm1, j];
                    b[mm1, j] = b[mm2, j];
                    b[mm2, j] = tmpD;
                }
            }
            void Multipl(int mm0, double val)
            {
                for(int j = 0; j < dof; j++)
                {
                    a[mm0, j] *= val;
                    b[mm0, j] *= val;
                }
            }
            void Substract(int mm1, int mm2)
            {
                //m1에서 m2를 뺀다.
                for (int j = 0; j < dof; j++)
                {
                    a[mm1, j] -= a[mm2, j];
                    b[mm1, j] -= b[mm2, j];
                }
            }
            int FindNonZeroDownSide(int mm0, int n)
            {
                for (int i = mm0; i < dof; i++)
                {
                    if (a[i, n] != 0) return i;
                }
                return -1;
            }
            void SubstractMultipl(int mm1, int mm2, double val)
            {
                //mm1행에서 mm2행에 val을 곱한 값을 뺀다.
                double[] tmpRowa = new double[dof];
                double[] tmpRowb = new double[dof];

                for (int j = 0; j < dof; j++)
                {
                    tmpRowa[j] = a[mm2, j] * val;
                    tmpRowb[j] = b[mm2, j] * val;
                }
                for (int j = 0; j < dof; j++)
                {
                    a[mm1, j] -= tmpRowa[j];
                    b[mm1, j] -= tmpRowb[j];
                }
            }
        }//Solver
        internal static double[,] IdentityMatrix(int dof)
        {
            double[,] identityMatrix = new double[dof, dof];
            for (int i = 0; i < dof; i++)
            {
                identityMatrix[i, i] = 1;
            }
            return identityMatrix;
        }

        public static List<int> ConvertStringsToIntList(string uInp)
        {
            List<int> iList = new List<int>();
            try
            {
                string[] split = uInp.Split(',');
                string[] pText = { "" , "" };
                foreach (string sText in split)
                {
                    if(sText.IndexOf("-") > 0)
                    {
                        pText = sText.Split('-');
                    }
                    else
                    {
                        if (sText.IndexOf("~") > 0)
                        {
                            pText = sText.Split('~');
                        }
                        else
                        {
                            iList.Add(Convert.ToInt32(sText));
                            break;
                        }
                    }
                    int str = Convert.ToInt32(pText[0]);
                    int end = Convert.ToInt32(pText[1]);
                    for(int i = str; i < end; i++)
                    {
                        iList.Add(i);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return iList;
        }

        public static Vector3D RotationVector3D(Vector3D vec, in Vector3D baseAxis, in double rotationAngleDeg)
        {
            Vector3D axisZ = baseAxis;
            axisZ.Normalize();
            Vector3D axisY = Vector3D.CrossProduct(axisZ, vec);
            axisY.Normalize();
            Vector3D axisX = Vector3D.CrossProduct(axisY, axisZ);
            axisX.Normalize();

            double rad = rotationAngleDeg / 180 * Math.PI;
            vec = Math.Cos(rad) * axisX + Math.Sin(rad) * axisY;
            return vec;
        }
        public static Vector3D RotateVector3D(Vector3D vec, Point3D basePoint, Vector3D baseAxis, double rotationAngleRad)
        {
            // To rotate the point (x,y,z) about the line through (a,b,c) with the normalised (u^2 + v^2 + w^2 = 1) direction vector
            // by the angle theta use the following function:
            double bAX2 = baseAxis.X * baseAxis.X;
            double bAY2 = baseAxis.Y * baseAxis.Y;
            double bAZ2 = baseAxis.Z * baseAxis.Z;
            double bApXYZ2 = baseAxis.X * vec.X + baseAxis.Y * vec.Y + baseAxis.Z * vec.Z;
            double cosRa = Math.Cos(rotationAngleRad);
            double sinRa = Math.Sin(rotationAngleRad);
            double bPXAX = basePoint.X * baseAxis.X;
            double bPYAY = basePoint.Y * baseAxis.Y;
            double bPZAZ = basePoint.Z * baseAxis.Z;
            Vector3D rotatedVector = new Vector3D
            {
                X = (basePoint.X * (bAY2 + bAZ2) - baseAxis.X * (bPYAY + bPZAZ - bApXYZ2)) * (1 - cosRa) + vec.X * cosRa + (-basePoint.Z * baseAxis.Y + basePoint.Y * baseAxis.Z - baseAxis.Z * vec.Y + baseAxis.Y * vec.Z) * sinRa,
                Y = (basePoint.Y * (bAX2 + bAZ2) - baseAxis.Y * (bPXAX + bPZAZ - bApXYZ2)) * (1 - cosRa) + vec.Y * cosRa + (basePoint.Z * baseAxis.X - basePoint.X * baseAxis.Z + baseAxis.Z * vec.X - baseAxis.X * vec.Z) * sinRa,
                Z = (basePoint.Z * (bAX2 + bAY2) - baseAxis.Z * (bPXAX + bPYAY - bApXYZ2)) * (1 - cosRa) + vec.Z * cosRa + (-basePoint.Y * baseAxis.X + basePoint.X * baseAxis.Y - baseAxis.Y * vec.X + baseAxis.X * vec.Y) * sinRa
            };
            return rotatedVector;
        }
        /// <summary>
        /// 평면과 직선의 교차점 반환.
        /// 평면이 lineP0와 lineP1 사이에 있지 않아도 무한 직선과의 교차점을 반환하므로 주의!
        /// </summary>
        /// <param name="planeAxis"></param>
        /// <param name="planePoint"></param>
        /// <param name="lineP0"></param>
        /// <param name="lineP1"></param>
        /// <returns></returns>
        internal static Point3D CrossPointBetweenPlaneAndInfiniteLine(Vector3D planeAxis, Point3D planePoint, Point3D lineP0, Point3D lineP1)
        {
            //ref. http://www.gisdeveloper.co.kr/?p=792
            double u = Vector3D.DotProduct(planeAxis, planePoint - lineP0) / Vector3D.DotProduct(planeAxis, lineP1 - lineP0);
            Point3D p = lineP0 + u * (lineP1 - lineP0);
            return p;
        }
        /// <summary>
        /// 평면과 직선의 교차점 반환.
        /// 평면이 lineP0와 lineP1 사이에 있지 않은 경우 (0,0,0)을 반환함. 왜 null 안되지? nan이라도 넣으려고 했는데 강제로는 안들어가네...
        /// </summary>
        /// <param name="planeAxis"></param>
        /// <param name="planePoint"></param>
        /// <param name="lineP0"></param>
        /// <param name="lineP1"></param>
        /// <returns></returns>
        internal static Point3D CrossPointBetweenPlaneAndLine(Vector3D planeAxis, Point3D planePoint, Point3D lineP0, Point3D lineP1)
        {
            //ref. http://www.gisdeveloper.co.kr/?p=792
            double u = Vector3D.DotProduct(planeAxis, planePoint - lineP0) / Vector3D.DotProduct(planeAxis, lineP1 - lineP0);
            Point3D p = new Point3D();
            if(0 <= u & u <= 1.0) p= lineP0 + u * (lineP1 - lineP0);
            return p;
        }
        public static Point3D CrossPoint_3Planes(Vector3D v1, Point3D p1, Vector3D v2, Point3D p2, Vector3D v3, Point3D p3)
        {
            Vector3D crossLineP1P2 = Vector3D.CrossProduct(v1, v2);
            Vector3D p1toCrossLine = Vector3D.CrossProduct(crossLineP1P2, v1);
            Point3D crossPointP1P2 = CrossPointBetweenPlaneAndInfiniteLine(v2, p2, p1, p1 + p1toCrossLine);
            Point3D crossPoint3Planes = CrossPointBetweenPlaneAndInfiniteLine(v3, p3, crossPointP1P2, crossPointP1P2 + crossLineP1P2);
            return crossPoint3Planes;
        }
        /// <summary>
        /// point가 planeAxis의 어느 위치에 있는지 반환.
        /// </summary>
        /// <param name="planeAxis"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static double PlanePosition(Vector3D planeAxis, Point3D point)
        {
            //ref. https://m.blog.naver.com/PostView.nhn?blogId=joy3x94&logNo=70145080536&proxyReferer=https:%2F%2Fwww.google.com%2F
            Point3D P = new Point3D(0, 0, 0);
            Point3D A = point;
            Vector3D u = planeAxis;

            Vector3D PA = A - P;
            if (PA.Length == 0) return 0;

            double theta = GF.AngleBetweenTwoVectors(PA, u);
            if (theta == 0)
            {
                Vector3D vec = point - new Point3D(0, 0, 0);
                return vec.Length;
            }
            double d = Vector3D.CrossProduct(PA, u).Length / u.Length;
            double lengthPH = d / Math.Tan(theta);
            return lengthPH;
        }
        /// <summary>
        /// 두 벡터 사이의 각(rad)을 반환합니다.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns>반환되는 값은 radian입니다.</returns>
        internal static double AngleBetweenTwoVectors(Vector3D v1, Vector3D v2)
        {
            //radian으로 반환
            double v1v2overv1lv2l = Vector3D.DotProduct(v1, v2) / (v1.Length * v2.Length);
            double rad = Math.Acos(v1v2overv1lv2l);
            //if (double.IsNaN(rad))
            //{
            //
            //}
            return rad;
        }
        /// <summary>
        /// quaryPoint가 plane의 위쪽에 있는지 확인. 위에 있으면 true, 아래면 false.
        /// </summary>
        internal static bool IsPointUpperPlane(Point3D quaryPoint, Point3D planePoint, Vector3D planeVector)
        {
            Vector3D vq = quaryPoint - planePoint;
            double ang = AngleBetweenTwoVectors(planeVector, vq);
            if (ang > Math.PI / 2) return false;
            return true;
        }
    }

    public class SectionPoly : List<SectionPolyPoint>
    {
        internal int numPoint = 0;
        public SectionPolyPoint Add(double x, double y)
        {
            SectionPolyPoint spp = new SectionPolyPoint(x, y);

            numPoint += 1;

            base.Add(spp);
            return spp;
        }
    }
    public class SectionPolyPoint
    {
        public double X;
        public double Y;
        public SectionPolyPoint(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    class MatrixDeterminant
    {
        public static void Main2()
        {
            try
            {
                //get the order of determinant from the user
                Console.WriteLine("Enter the order of determinant: ");
                int n = int.Parse(Console.ReadLine().ToString());
                Console.WriteLine("Order of determinant entered: " + n.ToString());
                if (n > 0)
                {
                    double[,] myMatrix = new double[n, n];
                    //input the matrix elements
                    for (int i = 0; i < n; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            Console.WriteLine("Enter element [" + (i + 1) + "]" + "[" + (j + 1) + "]: ");
                            myMatrix[i, j] = double.Parse(Console.ReadLine().ToString());
                        }
                    }
                    //display the entered matrix
                    Console.WriteLine("Matrix entered: ");
                    for (int i = 0; i < n; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            Console.Write(myMatrix[i, j].ToString() + " ");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine("Value of the determinant is: " + Determinant(myMatrix));
                }
                else
                {
                    Console.WriteLine("Order should be a positive integer.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }
        //this method determines the sign of the elements
        static int SignOfElement(int i, int j)
        {
            if ((i + j) % 2 == 0)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
        //this method determines the sub matrix corresponding to a given element
        static double[,] CreateSmallerMatrix(double[,] input, int i, int j)
        {
            int order = int.Parse(System.Math.Sqrt(input.Length).ToString());
            double[,] output = new double[order - 1, order - 1];
            int x = 0, y = 0;
            for (int m = 0; m < order; m++, x++)
            {
                if (m != i)
                {
                    y = 0;
                    for (int n = 0; n < order; n++)
                    {
                        if (n != j)
                        {
                            output[x, y] = input[m, n];
                            y++;
                        }
                    }
                }
                else
                {
                    x--;
                }
            }
            return output;
        }
        //this method determines the value of determinant using recursion
        static double Determinant(double[,] input)
        {
            int order = int.Parse(System.Math.Sqrt(input.Length).ToString());
            if (order > 2)
            {
                double value = 0;
                for (int j = 0; j < order; j++)
                {
                    double[,] Temp = CreateSmallerMatrix(input, 0, j);
                    value = value + input[0, j] * (SignOfElement(0, j) * Determinant(Temp));
                }
                return value;
            }
            else if (order == 2)
            {
                return ((input[0, 0] * input[1, 1]) - (input[1, 0] * input[0, 1]));
            }
            else
            {
                return (input[0, 0]);
            }
        }
    }

}