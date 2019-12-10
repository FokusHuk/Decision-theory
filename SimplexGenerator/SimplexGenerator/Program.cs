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
    }
}
