using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MAIGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            const string path = @"C:\Users\Ilya Axenov\source\repos\MAIGenerator\MAIGenerator\InputData.txt";

            double[] ConsistencyIndexesTable = new double[15] { 0.0, 0.0, 0.58, 0.9, 1.12, 1.24, 1.32, 1.41, 1.45, 1.49, 1.51, 1.48, 1.56, 1.57, 1.59 };

            double CILimit = 0.1;

            int CriteriasNumber;
            int AlternativesNumber;
            string[] CriteriasNames;
            string[] AlternativesNames;
            double[,] AimTable;
            List<double[,]> CriteriasTables;

            double[] AimPriorityVector;
            List<double[]> CriteriasPriorityVectors;
            double AimConsistencyIndex;
            List<double> CriteriasConsistencyIndexes;
            double[] AlternativesPriorities;

            GetDataFromFile(path, out CriteriasNumber, out CriteriasNames, out AlternativesNumber, out AlternativesNames, out AimTable, out CriteriasTables);
            if (CriteriasNumber == -1)
                return;

            AimPriorityVector = GetAimPriorityVector(AimTable, CriteriasNumber);
            Console.WriteLine("Вектор приоритетов:");
            foreach (double pv in AimPriorityVector)
                Console.Write("{0}\t", pv);

            CriteriasPriorityVectors = GetCriteriasPriorityVectors(CriteriasTables, AlternativesNumber, CriteriasNumber);
            Console.WriteLine("\n\nВектора приоритетов для каждого критерия:");
            for (int i = 0; i < CriteriasNumber; i++)
            {
                Console.Write("{0, -25}:\t", CriteriasNames[i]);
                foreach (double value in CriteriasPriorityVectors[i])
                    Console.Write("{0}\t", value);
                Console.WriteLine();
            }

            AimConsistencyIndex = GetAimConsistencyIndex(AimTable, CriteriasNumber, AimPriorityVector);
            Console.WriteLine("\nИндекс согласованности для матрицы \"цели\": {0}", AimConsistencyIndex);
            Console.WriteLine("Отношение согласованности: {0}", Math.Round(AimConsistencyIndex / ConsistencyIndexesTable[CriteriasNumber - 1], 2));
            if (Math.Round(AimConsistencyIndex / ConsistencyIndexesTable[CriteriasNumber - 1], 2) > CILimit)
            {
                Console.WriteLine("Отношение согласованности выше допустимого предела!");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("\nИндексы согласованности для матриц критериев:\n");
            CriteriasConsistencyIndexes = GetCriteriasConsistencyIndexes(CriteriasTables, CriteriasNumber, AlternativesNumber, CriteriasPriorityVectors);
            for (int i = 0; i < CriteriasNumber; i ++)
            {
                Console.WriteLine(CriteriasNames[i] + ": " + CriteriasConsistencyIndexes[i]);
                double CO = Math.Round(CriteriasConsistencyIndexes[i] / ConsistencyIndexesTable[AlternativesNumber - 1], 2);
                if (CO > 0.1)
                    CO = Math.Round(CO, 1);
                Console.WriteLine("Отношение согласованности для {0}: {1}\n", CriteriasNames[i], CO);
                if (CO > CILimit)
                {
                    Console.WriteLine("Отношение согласованности выше допустимого предела!");
                    Console.ReadLine();
                    return;
                }
            }

            Console.WriteLine("Синтез альтернатив.\n");

            AlternativesPriorities = GetAlternativesPriorities(AimPriorityVector, CriteriasPriorityVectors, CriteriasNumber, AlternativesNumber);

            Console.WriteLine("Приоритеты альтернатив:\n");
            for (int i = 0; i < AlternativesNumber; i ++)
            {
                Console.WriteLine(AlternativesNames[i] + ": " + AlternativesPriorities[i]);
            }

            Console.WriteLine("\nНаилучший вариант: {0}", AlternativesNames[GetBestAlternativeIndex(AlternativesPriorities)]);

            Console.ReadLine();
        }

        static void GetDataFromFile(string path, out int CriteriasNumber, out string[] CriteriasNames, out int AlternativesNumber,
                                    out string[] AlternativesNames, out double[,] AimTable, out List<double[,]> CriteriasTables)
        {
            CriteriasNumber = -1;
            CriteriasNames = null;
            AlternativesNumber = -1;
            AlternativesNames = null;
            AimTable = null;
            CriteriasTables = null;

            try
            {
                StreamReader reader = new StreamReader(@path);

                CriteriasNumber = Convert.ToInt32(reader.ReadLine().Split(' ')[2]); // Количество критериев:

                reader.ReadLine(); // Названия критериев:
                CriteriasNames = new string[CriteriasNumber];
                for (int i = 0; i < CriteriasNumber; i++)
                {
                    CriteriasNames[i] = reader.ReadLine();
                }

                AlternativesNumber = Convert.ToInt32(reader.ReadLine().Split(' ')[2]); // Количество альтернатив:

                reader.ReadLine(); // Названия альтернатив:
                AlternativesNames = new string[AlternativesNumber];
                for (int i = 0; i < AlternativesNumber; i++)
                {
                    AlternativesNames[i] = reader.ReadLine();
                }

                reader.ReadLine(); // Таблица "цели":
                AimTable = new double[CriteriasNumber, CriteriasNumber];
                for (int i = 0; i < CriteriasNumber; i++)
                {
                    string[] line = reader.ReadLine().Replace("\t\t", " ").Split(' ');

                    for (int j = 0; j < CriteriasNumber; j++)
                    {
                        if (line[j].Contains("/"))
                        {
                            AimTable[i, j] = Math.Round(Convert.ToDouble(line[j].Split('/')[0]) / Convert.ToDouble(line[j].Split('/')[1]), 2);
                        }
                        else
                        {
                            AimTable[i, j] = Convert.ToDouble(line[j]);
                        }
                    }
                }

                CriteriasTables = new List<double[,]>();

                for (int i = 0; i < CriteriasNumber; i++)
                {
                    reader.ReadLine(); // Таблица i критерия:
                    CriteriasTables.Add(new double[AlternativesNumber, AlternativesNumber]);
                    for (int j = 0; j < AlternativesNumber; j++)
                    {
                        string[] line = reader.ReadLine().Replace("\t\t", " ").Split(' ');

                        for (int k = 0; k < AlternativesNumber; k++)
                        {
                            if (line[k].Contains("/"))
                            {
                                CriteriasTables[i][j, k] = Math.Round(Convert.ToDouble(line[k].Split('/')[0]) / Convert.ToDouble(line[k].Split('/')[1]), 2);
                            }
                            else
                            {
                                CriteriasTables[i][j, k] = Convert.ToDouble(line[k]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка чтения входных данных: \n" + ex.Message);
                CriteriasNumber = -1;
                Console.ReadLine();
            }
        }

        static double[] GetAimPriorityVector(double[,] AimTable, int CriteriasNumber)
        {
            double[] PriorityVector = new double[CriteriasNumber];
            double geometryAvg = 0.0;
            double[] V = new double[CriteriasNumber];

            for (int i = 0; i < CriteriasNumber; i++)
            {
                double Vi = 1;

                for (int j = 0; j < CriteriasNumber; j++)
                {
                    Vi *= AimTable[i, j];
                }

                V[i] = Math.Round(Math.Pow(Vi, (1.0 / (double)CriteriasNumber)), 2);

                geometryAvg += V[i];
            }

            for (int i = 0; i < CriteriasNumber; i++)
            {
                PriorityVector[i] = Math.Round(V[i] / geometryAvg, 2);
            }

            return PriorityVector;
        }

        static List<double[]> GetCriteriasPriorityVectors(List<double[,]> CriteriasTables, int AlternativesNumber, int CriteriasNumber)
        {
            List<double[]> CriteriasPriorityVectors = new List<double[]>();
            double geometryAvg = 0.0;
            double[] V = new double[AlternativesNumber];

            for (int i = 0; i < CriteriasNumber; i++)
            {
                geometryAvg = 0.0;

                for (int j = 0; j < AlternativesNumber; j++)
                {
                    double Vi = 1;

                    for (int k = 0; k < AlternativesNumber; k++)
                    {
                        Vi *= CriteriasTables[i][j, k];
                    }

                    V[j] = Math.Round(Math.Pow(Vi, (1.0 / (double)AlternativesNumber)), 3);

                    geometryAvg += V[j];
                }

                CriteriasPriorityVectors.Add(new double[AlternativesNumber]);

                for (int j = 0; j < AlternativesNumber; j++)
                {
                    CriteriasPriorityVectors[i][j] = Math.Round(V[j] / geometryAvg, 3);
                }
            }

            return CriteriasPriorityVectors;
        }

        static double GetAimConsistencyIndex(double[,] AimTable, int CriteriasNumber, double[] AimPriorityVector)
        {
            double AimConsistencyIndex = 0.0;
            double[] P = new double[CriteriasNumber];
            double PSum = 0.0;

            for (int i = 0; i < CriteriasNumber; i ++)
            {
                double S = 0.0;

                for (int j = 0; j < CriteriasNumber; j ++)
                {
                    S += AimTable[j, i];
                }

                P[i] = Math.Round(S * AimPriorityVector[i], 2);
            }

            foreach (double p in P)
                PSum += p;

            AimConsistencyIndex = Math.Round((PSum - CriteriasNumber) / (CriteriasNumber - 1), 4);

            return AimConsistencyIndex;
        }

        static List<double> GetCriteriasConsistencyIndexes(List<double[,]> CriteriasTables, int CriteriasNumber, int AlternativesNumber,
                                                            List<double[]> CriteriasPriorityVectors)
        {
            List<double> CriteriasConsistencyIndexes = new List<double>();
            double[] P = new double[AlternativesNumber];
            double PSum = 0.0;

            for (int i = 0; i < CriteriasNumber; i ++)
            {
                PSum = 0.0;
                for (int j = 0; j < AlternativesNumber; j++)
                {
                    double S = 0.0;

                    for (int k = 0; k < AlternativesNumber; k++)
                    {
                        S += CriteriasTables[i][k, j];
                    }

                    P[j] = Math.Round(S * CriteriasPriorityVectors[i][j], 2);
                }

                foreach (double p in P)
                    PSum += p;

                CriteriasConsistencyIndexes.Add(Math.Round((PSum - AlternativesNumber) / (AlternativesNumber - 1), 4));
            }

            return CriteriasConsistencyIndexes;
        }

        static double[] GetAlternativesPriorities(double[] AimPriorityVector, List<double[]> CriteriasPriorityVectors, int CriteriasNumber, 
                                                    int AlternativesNumber)
        {
            double[] AlternativesPriorities = new double[AlternativesNumber];

            for (int i = 0; i < CriteriasNumber; i++)
                AlternativesPriorities[i] = 0.0;

            for (int i = 0; i < AlternativesNumber; i ++)
            {
                for (int k = 0; k < CriteriasNumber; k ++)
                {
                    AlternativesPriorities[i] += Math.Round(AimPriorityVector[k] * CriteriasPriorityVectors[k][i], 3);
                }
            }

            return AlternativesPriorities;
        }

        static int GetBestAlternativeIndex(double[] AlternativesPriorities)
        {
            double max = AlternativesPriorities.Max();

            for (int i = 0; i < AlternativesPriorities.Length; i++)
                if (AlternativesPriorities[i] == max)
                    return i;

            return 0;
        }
    }
}
