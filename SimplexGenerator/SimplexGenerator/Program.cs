using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SimplexGenerator
{
    class Program
    {
        static List<string> FactorNames = new List<string>(); // имена x-ов
        static List<int> Factors = new List<int>(); // значения при x
        static int X; // размер
        static int Y; // таблицы
        static double[,] XTable; // значения ограничений
        static List<string> XVars = new List<string>();
        static List<string> YVars = new List<string>();
        static List<double> Delta = new List<double>();
        static int Iteration = -1;

        // Для двойственной задачи
        static List<int> StartFactors = new List<int>(); // первоначальные значения при x
        static double[,] StartXTable; // значения ограничений


        static void Main(string[] args)
        {
            Init();

            CalculateDelta();

            ShowSimplexTable();

            while (!IsOptimized())
            {
                CreateNewTable();
                ShowSimplexTable();
            }

            InverseMethod();

            Console.ReadLine();
        }

        static void Init()
        {
            string path = @"C:\Users\Ilya Axenov\source\repos\SimplexGenerator\SimplexGenerator\InputData.txt";
            StreamReader reader = new StreamReader(@path);

            string[] function = reader.ReadLine().Split(' ');
            foreach (string s in function)
            {
                if (s.Contains("x") && !s.Contains("f"))
                {
                    if (s.IndexOf("x") == 1 && s[0] == '-')
                        Factors.Add(-1);
                    else if (s.IndexOf("x") == 0)
                        Factors.Add(1);
                    else
                        Factors.Add(Convert.ToInt32(s.Substring(0, s.IndexOf("x"))));
                }
            }

            X = Convert.ToInt32(reader.ReadLine());
            Y = Factors.Count + 1;

            XTable = new double[X, Y];

            for (int i = 0; i < X; i++)
                for (int j = 0; j < Y; j++)
                    XTable[i, j] = 0;

            for (int i = 0; i < X; i ++)
            {
                function = reader.ReadLine().Split(' ');
                foreach (string s in function)
                {
                    if (s.Contains("x"))
                    {
                        int index = Convert.ToInt32(s.Substring(s.IndexOf("x") + 1)) - 1;
                        if (s.IndexOf("x") == 1 && s[0] == '-')
                            XTable[i, index] = -1;
                        else if (s.IndexOf("x") == 0)
                            XTable[i, index] = 1;
                        else
                            XTable[i, index] = Convert.ToInt32(s.Substring(0, s.IndexOf("x")));
                    }
                }
                XTable[i, Y - 1] = Convert.ToInt32(function[function.Length - 1]);
            }

            for (int i = 0; i < X; i++)
                Factors.Add(0);

            for (int i = 0; i < Factors.Count; i++)
                FactorNames.Add("x" + (i + 1));

            for (int i = 0; i < Y - 1; i++)
                YVars.Add("x" + (i + 1));
            YVars.Add("A0");

            for (int i = 0; i < X; i++)
                XVars.Add("x" + (Y + i));

            reader.Close();

            StartFactors = Factors;
            StartXTable = XTable;
        }
        
        static void ShowState()
        {
            foreach (string a in FactorNames)
                Console.Write("{0, 2} ", a);
            Console.WriteLine();

            foreach (int a in Factors)
                Console.Write("{0, 2} ", a);
            Console.WriteLine("\n");

            for (int i = 0; i < X; i++)
            {
                for (int j = 0; j < Y; j++)
                    Console.Write(XTable[i, j] + " ");
                Console.WriteLine();
            }
            Console.WriteLine();

            for (int i = 0; i < YVars.Count; i++)
                Console.Write(YVars[i] + " ");
            Console.WriteLine("\n");

            for (int i = 0; i < XVars.Count; i++)
                Console.Write(XVars[i] + " ");
            Console.WriteLine("\n");

            foreach (int a in Delta)
                Console.Write("{0, 2} ", a);
            Console.WriteLine("\n");
        }

        static void CalculateDelta()
        {
            for (int i = 0; i < Y; i ++)
            {
                double delta = 0;

                for (int j = 0; j < X; j ++)
                {
                    delta += XTable[j, i] * Factors[FactorNames.IndexOf(XVars[j])];
                }

                if (i != Y - 1)
                    delta -= Factors[i];

                Delta.Add(delta);
            }
        }

        static void ShowSimplexTable()
        {
            if (Iteration != -1)
                Console.WriteLine("<=====\t\tSIMPLEX TABLE {0}\t\t=====>", Iteration);
            else
                Console.WriteLine("<=====\t      SIMPLEX TABLE INIT\t=====>");
            Iteration++;

            Console.Write("{0, 5} ", "");
            Console.Write("{0, 5} ", "cj");
            for (int i = 0; i < Y - 1; i++)
                Console.Write("{0, 5} ", Factors[FactorNames.IndexOf(YVars[i])]);
            Console.WriteLine();

            Console.Write("{0, 5} ", "Cb");
            Console.Write("{0, 5} ", "");
            foreach (string a in YVars)
                Console.Write("{0, 5} ", a);
            Console.WriteLine();

            for (int i = 0; i < X; i ++)
            {
                Console.Write("{0, 5} ", Factors[FactorNames.IndexOf(XVars[i])]);
                Console.Write("{0, 5} ", XVars[i]);
                for (int j = 0; j < Y; j++)
                    Console.Write("{0, 5} ", Math.Round(XTable[i, j], 1));
                Console.WriteLine();
            }

            Console.Write("{0, 5} ", "");
            Console.Write("{0, 5} ", "f");
            for (int i = 0; i < Y; i ++)
                Console.Write("{0, 5} ", Math.Round(Delta[i], 1));
            Console.WriteLine();

            Console.Write("{0, 5} ", "");
            Console.Write("{0, 5} ", "");
            for (int i = 0; i < Y - 1; i ++)
                Console.Write("{0, 5} ", "d" + (i + 1).ToString());
            Console.Write("{0, 5} ", "Q");
            Console.WriteLine();

            Console.WriteLine("<=====\t\t             \t\t=====>\n");
        }

        static bool IsOptimized()
        {
            for (int i = 0; i < Y - 1; i++)
                if (Delta[i] < 0)
                    return false;

            return true;
        }

        static void CreateNewTable()
        {
            // определение разрешающего элемента
            int Xel = 0, Yel = 0;

            for (int i = 1; i < Y - 1; i++)
                if (Delta[i] < Delta[Yel])
                    Yel = i;

            for (int i = 0; i < X; i++)
                if (XTable[i, Yel] > 0)
                {
                    Xel = i;
                    break;
                }

            for (int i = Xel + 1; i < X; i ++)
            {
                if (XTable[i, Yel] > 0 && XTable[i, Y - 1] / XTable[i, Yel] < XTable[Xel, Y - 1] / XTable[Xel, Yel])
                    Xel = i;
            }

            string temp = YVars[Yel];
            YVars[Yel] = XVars[Xel];
            XVars[Xel] = temp;

            // создание новой таблицы
            // расчет элементов в строке и столбце разрешающего элемента
            double[,] NewXTable = new double[X, Y];

            NewXTable[Xel, Yel] = 1 / XTable[Xel, Yel];

            for (int i = 0; i < X; i++)
                if (i != Xel)
                    NewXTable[i, Yel] = -XTable[i, Yel] / XTable[Xel, Yel];            

            for (int i = 0; i < Y; i ++)
                if (i != Yel)
                    NewXTable[Xel, i] = XTable[Xel, i] / XTable[Xel, Yel];

            // расчет остальных элементов по правилу прямоугольника
            for (int i = 0; i < X; i++)
            {
                if (i == Xel)
                    continue;

                for (int j = 0; j < Y; j++)
                {
                    if (j == Yel)
                        continue;

                    NewXTable[i, j] = (XTable[i, j] * XTable[Xel, Yel] - XTable[i, Yel] * XTable[Xel, j]) / XTable[Xel, Yel];
                }
            }

            for (int i = 0; i < Y - 1; i++)
                if (i != Yel)
                    Delta[i] = (Delta[i] * XTable[Xel, Yel] - Delta[Yel] * XTable[Xel, i]) / XTable[Xel, Yel];
            Delta[Yel] = -Delta[Yel] / XTable[Xel, Yel];

            XTable = NewXTable;

            Delta[Y - 1] = 0;
            for (int j = 0; j < X; j++)
            {
                Delta[Y - 1] += XTable[j, Y - 1] * Factors[FactorNames.IndexOf(XVars[j])];
            }
        }


        static void InverseMethod()
        {
            Console.WriteLine("\nДвойственная задача. Первая теорема.\n");

            double[,] TransXTable = new double[Y - 1, X];
            double[,] Dmatrix = new double[X, X];
            double[,] ReverseMatrix = new double[X, X];
            double[] Yfinal = new double[X];

            Console.Write("g(y) = ");
            for (int i = 0; i < X; i ++)
            {
                if (i == X - 1)
                    Console.WriteLine(StartXTable[i, Y - 1] + "y" + (i + 1) + "\n");
                else
                    Console.Write(StartXTable[i, Y - 1] + "y" + (i + 1) + " + ");
            }

            // Транспонирование матрицы
            for (int i = 0; i < Y - 1; i++)
                for (int j = 0; j < X; j++)
                    TransXTable[i, j] = StartXTable[j, i];

            // Ограничения двойственной функции
            for (int i = 0; i < Y - 1; i++)
            {
                for (int j = 0; j < X; j++)
                {
                    if (j == X - 1)
                        Console.Write(TransXTable[i, j] + "y" + (j + 1));
                    else
                        Console.Write(TransXTable[i, j] + "y" + (j + 1) + " + ");
                }
                Console.WriteLine(" > " + StartFactors[i]);
            }

            // Составление D-матрицы
            for (int i = 0; i < X; i ++)
            {
                int index = FactorNames.IndexOf(XVars[i]);

                if (index < X)
                {
                    for (int j = 0; j < X; j++)
                        Dmatrix[j, i] = StartXTable[j, index];
                }   
                else
                {
                    index = Convert.ToInt32(XVars[i].Substring(1)) - Y;

                    for (int j = 0; j < X; j ++)
                    {
                        if (j != index)
                            Dmatrix[j, i] = 0;
                        else
                            Dmatrix[j, i] = 1;
                    }
                }
            }

            // Вычисление обратной матрицы
            double det = GetMatrixDeterminant(Dmatrix, X, -1, -1);

            if (det != 0)
            {
                double[,] TransDmatrix = new double[X, X];

                for (int i = 0; i < X; i++)
                    for (int j = 0; j < X; j++)
                        TransDmatrix[i, j] = Dmatrix[j, i];      

                for (int i = 0; i < X; i++)
                    for (int j = 0; j < X; j++)
                    {
                        ReverseMatrix[i, j] = (Math.Pow(-1, i + j) * GetMatrixDeterminant(TransDmatrix, X, i, j)) / det;
                    }

                Console.WriteLine("\nОбратная матрица D:");
                for (int i = 0; i < X; i++)
                {
                    for (int j = 0; j < X; j++)
                        Console.Write(Math.Round(ReverseMatrix[i, j], 2) + "\t");
                    Console.WriteLine();
                }
                Console.WriteLine();
            }

            for (int i = 0; i < X; i++)
                Yfinal[i] = 0;

            for (int i = 0; i < X; i++)
                for (int j = 0; j < X; j++)
                {
                    Yfinal[i] += Factors[FactorNames.IndexOf(XVars[j])] * ReverseMatrix[j, i];
                }

            Console.WriteLine("\nЗапасы ресурсов:");
            for (int i = 0; i < X; i++)
                Console.WriteLine(Math.Round(Yfinal[i], 2) + " ");

            Console.Write("\nМинимальное значения целевой функции двойственной задачи: ");
            double result = 0;
            for (int i = 0; i < X; i ++)
            {
                result += StartXTable[i, Y - 1] * Yfinal[i];
            }
            Console.WriteLine(Math.Round(result, 1));
        }

        static double GetMatrixDeterminant(double[,] dm, int size, int x, int y)
        {
            if (x != - 1)
            {
                double[,] newdm = new double[size - 1, size - 1];

                int ii = 0, jj = 0;
                for (int i = 0; i < size; i++)
                {
                    if (i == x) continue;
                    for (int j = 0; j < size; j++)
                    {
                        if (j == y) continue;
                        newdm[ii, jj] = dm[i, j];
                        jj++;
                    }
                    ii++;
                    jj = 0;
                }

                return GetMatrixDeterminant(newdm, size - 1, -1, -1);
            }
            else if (size == 2)
            {
                return dm[0, 0] * dm[1, 1] - dm[1, 0] * dm[0, 1];
            }
            else if (size == 3)
            {
                return dm[0, 0] * dm[1, 1] * dm[2, 2] + dm[1, 0] * dm[2, 1] * dm[0, 2] + dm[0, 1] * dm[1, 2] * dm[2, 0] -
                    dm[0, 2] * dm[1, 1] * dm[2, 0] - dm[2, 1] * dm[1, 2] * dm[0, 0] - dm[1, 0] * dm[0, 1] * dm[2, 2];
            }
            else
            {
                double det = 0.0;
                int line = 0;
                for (int i = 0; i < size; i ++)
                    for (int j = 0; j < size; j ++)
                        if (dm[i, j] != 0)
                        {
                            line = i;
                            break;
                        }
                for (int j = 0; j < size; j++)
                    det += dm[line, j] * Math.Pow(-1, j + line) * GetMatrixDeterminant(dm, size, line, j);

                return det;
            }
        }
    }
}
