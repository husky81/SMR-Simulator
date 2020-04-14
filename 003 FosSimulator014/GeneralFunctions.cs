﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace _003_FosSimulator014
{
    static class GF
    {

        public static double MatrixDeterminant(double[,] matrix)
        {
            int[] perm;
            int toggle;
            double[,] lum = MatrixDecompose(matrix, out perm, out toggle);
            if (lum == null)
                throw new Exception("Unable to compute MatrixDeterminant");
            double result = toggle;
            for (int i = 0; i < lum.GetLength(0); ++i)
                result *= lum[i, i];

            return result;
        }
        private static double[,] MatrixDecompose(double[,] matrix, out int[] perm, out int toggle)
        {
            // Doolittle LUP decomposition with partial pivoting.
            // rerturns: result is L (with 1s on diagonal) and U; perm holds row permutations; toggle is +1 or -1 (even or odd)
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            //Check if matrix is square
            if (rows != cols)
                throw new Exception("Attempt to MatrixDecompose a non-square mattrix");

            double[,] result = MatrixDuplicate(matrix); // make a copy of the input matrix

            perm = new int[rows]; // set up row permutation result
            for (int i = 0; i < rows; ++i) { perm[i] = i; } // i are rows counter

            toggle = 1; // toggle tracks row swaps. +1 -> even, -1 -> odd. used by MatrixDeterminant

            for (int j = 0; j < rows - 1; ++j) // each column, j is counter for coulmns
            {
                double colMax = Math.Abs(result[j, j]); // find largest value in col j
                int pRow = j;
                for (int i = j + 1; i < rows; ++i)
                {
                    if (result[i, j] > colMax)
                    {
                        colMax = result[i, j];
                        pRow = i;
                    }
                }

                if (pRow != j) // if largest value not on pivot, swap rows
                {
                    double[] rowPtr = new double[result.GetLength(1)];

                    //in order to preserve value of j new variable k for counter is declared
                    //rowPtr[] is a 1D array that contains all the elements on a single row of the matrix
                    //there has to be a loop over the columns to transfer the values
                    //from the 2D array to the 1D rowPtr array.
                    //----tranfer 2D array to 1D array BEGIN

                    for (int k = 0; k < result.GetLength(1); k++)
                    {
                        rowPtr[k] = result[pRow, k];
                    }

                    for (int k = 0; k < result.GetLength(1); k++)
                    {
                        result[pRow, k] = result[j, k];
                    }

                    for (int k = 0; k < result.GetLength(1); k++)
                    {
                        result[j, k] = rowPtr[k];
                    }

                    //----tranfer 2D array to 1D array END

                    int tmp = perm[pRow]; // and swap perm info
                    perm[pRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle; // adjust the row-swap toggle
                }

                if (Math.Abs(result[j, j]) < 1.0E-20) // if diagonal after swap is zero . . .
                    return null; // consider a throw

                for (int i = j + 1; i < rows; ++i)
                {
                    result[i, j] /= result[j, j];
                    for (int k = j + 1; k < rows; ++k)
                    {
                        result[i, k] -= result[i, j] * result[j, k];
                    }
                }
            } // main j column loop

            return result;
        } // MatrixDecompose

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

        private static double[,] MatrixDuplicate(double[,] matrix)
        {
            // allocates/creates a duplicate of a matrix. assumes matrix is not null.
            double[,] result = MatrixCreate(matrix.GetLength(0), matrix.GetLength(1));
            for (int i = 0; i < matrix.GetLength(0); ++i) // copy the values
                for (int j = 0; j < matrix.GetLength(1); ++j)
                    result[i, j] = matrix[i, j];
            return result;
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
    }

    class SectionPoly : List<SectionPolyPoint>
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
    class SectionPolyPoint
    {
        internal double X;
        internal double Y;
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