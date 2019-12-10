using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace ElectraGenerator
{
    class Criteria
    {
        private static int count = 1;

        public string Name { get; }
        public int ID { get; }
        public int Weight { get; }
        public int[] Scale { get; }
        public bool Vector { get; } // true - max, false - min

        public Criteria(string name, int weight, int scaleSize, bool vector)
        {
            Name = name;
            Weight = weight;
            Vector = vector;

            Scale = new int[scaleSize];
            for (int i = 0; i < scaleSize; i++)
                Scale[i] = Weight * (i + 1);

            ID = count;
            count++;
        }
    }

    class Headphones
    {
        public int[] Criterias { get; }

        public Headphones(List<int> criterias)
        {
            Criterias = new int[criterias.Count];
            criterias.CopyTo(Criterias);
        }
    }

    static class Program
    {
        public static Random _generator = new Random();

        static void Main(string[] args)
        {
            #region Критерии
            List<Criteria> criterias = new List<Criteria>();
            criterias.Add(new Criteria("Price", 5, 3, false)); // Цена
            criterias.Add(new Criteria("Rate", 5, 3, true)); // Рейтинг
            criterias.Add(new Criteria("WorkTime", 4, 3, true)); // Время работы
            criterias.Add(new Criteria("Sense", 3, 3, true)); // Чувствительность
            criterias.Add(new Criteria("Weight", 3, 2, false)); // Вес
            criterias.Add(new Criteria("MicQuality", 2, 2, true)); // Качество микрофона
            #endregion

            const string path = @"C:\Users\Ilya Axenov\source\repos\ElectraGenerator\ElectraGenerator\inputData.txt";
            string choice = "";
            List<Headphones> headphones = null;

            Console.WriteLine("Выберите способ задания таблицы:\n1 - из файла\n2 - случайные значения для N альтернатив");

            while (true)
            {
                Console.Write(">> ");
                try
                {
                    choice = Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Неправильный формат входных данных.\n" + ex.Message);
                }
                if (choice == "1" || choice == "2")
                    break;
            }

            switch (choice)
            {
                case "1":
                    headphones = GetHeadphonesFromFile(path);
                    break;
                case "2":
                    Console.WriteLine("Введите количество альтернатив: ");
                    int count = 0;
                    try
                    {
                        count = Convert.ToInt32(Console.ReadLine());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Неправильный формат входных данных.\n" + ex.Message);
                    }
                    if (count <= 5 || count > 10000)
                        count = 10;
                    headphones = GetRandomList(count, criterias);
                    break;
            }

            Console.WriteLine("\nСПИСОК АЛЬТЕРНАТИВ:\n");
            headphones.ShowList(criterias);

            double[,] pMatrix = headphones.GetPreferenceMatrix(criterias);

            Console.WriteLine("МАТРИЦА ПРЕДПОЧТЕНИЙ:\n");
            ShowPreferenceMatrix(pMatrix);

            Console.WriteLine("\nМАТРИЦА ПРЕДПОЧТЕНИЙ С ПОРОГОМ ОТБОРА ПРЕДПОЧТЕНИЙ\n");
            Console.Write("Введите порог отбора предпочтений: ");
            double threshold = 0;
            try
            {
                threshold = Convert.ToDouble(Console.ReadLine().Replace(".", ","));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Неправильный формат входных данных.\n" + ex.Message);
                threshold = 0;
            }
            pMatrix.SetThreshold(threshold);
            ShowPreferenceMatrix(pMatrix);

            Console.WriteLine("\nУРОВНИ ГРАФА ПРЕДПОЧТЕНИЙ:\n");
            ShowLevels(pMatrix);

            Console.ReadLine();
        }

        static List<Headphones> GetHeadphonesFromFile(string path)
        {
            List<Headphones> headphones = new List<Headphones>();
            List<int> criteriaValues = new List<int>();

            try
            {
                StreamReader reader = new StreamReader(@path);

                reader.ReadLine();

                string line = "";

                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();

                    line = line.Replace("\t\t\t", " ");

                    string[] data = line.Split(' ');

                    foreach (string d in data)
                        criteriaValues.Add(Convert.ToInt32(d));

                    headphones.Add(new Headphones(criteriaValues));

                    criteriaValues.Clear();

                    Thread.Sleep(50);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при чтении данных из файла.\n" + ex.Message);
            }

            return headphones;
        }

        static List<Headphones> GetRandomList(int size, List<Criteria> criterias)
        {
            List<Headphones> headphones = new List<Headphones>();
            List<int> criteriaValues = new List<int>();

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < criterias.Count; j++)
                {
                    criteriaValues.Add(criterias[j].Scale[_generator.Next(0, criterias[j].Scale.Length)]);
                }

                headphones.Add(new Headphones(criteriaValues));

                criteriaValues.Clear();

                Thread.Sleep(50);
            }

            return headphones;
        }

        static void ShowList(this List<Headphones> headphones, List<Criteria> criterias)
        {
            Console.Write("{0, -5}", "№");
            for (int i = 0; i < criterias.Count; i++)
                Console.Write("{0, -10}", criterias[i].Name);
            Console.WriteLine();

            for (int i = 0; i < headphones.Count; i++)
            {
                Console.Write("{0, -5}", i + 1);

                for (int j = 0; j < headphones[i].Criterias.Length; j++)
                {
                    Console.Write("{0, -10}", headphones[i].Criterias[j]);
                }

                Console.WriteLine();
            }

            Console.WriteLine();
        }

        static double[,] GetPreferenceMatrix(this List<Headphones> headphones, List<Criteria> criterias)
        {
            Headphones h1, h2;
            double P = 0, N = 0;

            double[,] pMatrix = new double[headphones.Count, headphones.Count];

            for (int i = 0; i < headphones.Count; i++)
            {
                h1 = headphones[i];

                for (int j = i + 1; j < headphones.Count; j++)
                {
                    h2 = headphones[j];

                    for (int k = 0; k < criterias.Count; k++)
                    {
                        if (criterias[k].Vector)
                        {
                            if (h1.Criterias[k] > h2.Criterias[k])
                                P += criterias[k].Weight;
                            else if (h1.Criterias[k] < h2.Criterias[k])
                                N += criterias[k].Weight;
                        }
                        else
                        {
                            if (h1.Criterias[k] < h2.Criterias[k])
                                P += criterias[k].Weight;
                            else if (h1.Criterias[k] > h2.Criterias[k])
                                N += criterias[k].Weight;
                        }
                    }

                    if (N != 0 && P / N > 1)
                    {
                        pMatrix[i, j] = Math.Round(P / N, 2);
                        pMatrix[j, i] = 0;
                    }
                    else if (P != 0 && N / P > 1)
                    {
                        pMatrix[i, j] = 0;
                        pMatrix[j, i] = Math.Round(N / P, 2);
                    }
                    else
                    {
                        pMatrix[i, j] = 0;
                        pMatrix[j, i] = 0;
                    }

                    P = 0;
                    N = 0;
                }
            }

            return pMatrix;
        }

        static void ShowPreferenceMatrix(double[,] pMatrix)
        {
            if (Math.Sqrt(pMatrix.Length) > 20)
                return;
            for (int i = 0; i < Math.Sqrt(pMatrix.Length); i++)
                if (i == 0)
                    Console.Write("     {0, -8}", i + 1);
                else
                    Console.Write("{0, -8}", i + 1);

            Console.WriteLine();

            for (int i = 0; i < Math.Sqrt(pMatrix.Length); i++)
            {
                Console.Write("{0, -4} ", i + 1);

                for (int j = 0; j < Math.Sqrt(pMatrix.Length); j++)
                    Console.Write("{0, -8}", pMatrix[i, j]);

                Console.WriteLine();
            }
        }

        static void SetThreshold(this double[,] pMatrix, double threshold)
        {
            for (int i = 0; i < Math.Sqrt(pMatrix.Length); i++)
                for (int j = 0; j < Math.Sqrt(pMatrix.Length); j++)
                    if (pMatrix[i, j] < threshold)
                        pMatrix[i, j] = 0;
        }

        static void ShowLevels(double[,] pMatrix)
        {
            bool isChanged = false;

            int[] levels = new int[Convert.ToInt32(Math.Sqrt(pMatrix.Length))];

            for (int i = 0; i < levels.Length; i++)
                levels[i] = 0;

            List<int> levelPeaks = null;

            levelPeaks = GetLevelVertices(pMatrix, 0);

            foreach (int lp in levelPeaks)
            {
                for (int j = 0; j < levels.Length; j++)
                    if (pMatrix[lp, j] != 0 && levels[lp] <= levels[j])
                        levels[j] = levels[lp] + 1;
            }           

            for (int i = 1; i < levels.Length; i ++)
            {
                levelPeaks.Clear();
                Console.Write("Уровень: " + i + "; ");
                for (int j = 0; j < levels.Length; j ++)
                    if (levels[j] == i)
                    {
                        levelPeaks.Add(j);
                        Console.Write((j + 1) + " ");
                    }
                Console.WriteLine();

                if (levelPeaks.Count == 0)
                    break;

                foreach (int lp in levelPeaks)
                {
                    for (int j = 0; j < levels.Length; j++)
                        if (pMatrix[lp, j] != 0 && levels[lp] >= levels[j])
                            levels[j] = levels[lp] + 1;

                    Console.Write("[ " + (lp + 1) + " ] ");
                    for (int k = 0; k < levels.Length; k++)
                        Console.Write(levels[k] + " ");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("\n\nУровни графа: ");
            for (int i = 0; i <= levels.Max(); i ++)
            {
                Console.Write("Уровень " + (i + 1) + ": ");
                for (int j = 0; j < levels.Length; j ++)
                {
                    if (levels[j] == i)
                        Console.Write((j + 1) + " ");
                }
                Console.WriteLine();
            }
        }

        static List<int> GetLevelVertices(double[,] pMatrix, int level)
        {
            List<int> zeros = new List<int>();
            int count = 0;

            for (int i = 0; i < Math.Sqrt(pMatrix.Length); i++)
            {
                count = 0;
                for (int j = 0; j < Math.Sqrt(pMatrix.Length); j++)
                {
                    if (pMatrix[j, i] != 0) count++;
                    if (count > level) break;
                    if (j == Convert.ToInt32(Math.Sqrt(pMatrix.Length)) - 1 && count == level)
                        zeros.Add(i);
                }
            }

            return zeros;
        }
    }
}
