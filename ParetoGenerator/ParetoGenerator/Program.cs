using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace ParetoGenerator
{
    struct Headphones
    {
        public Headphones(int price, double rate, int workTime, int sensitivity, int weight)
        {
            Price = price;
            Rate = rate;
            WorkTime = workTime;
            Sensitivity = sensitivity;
            Weight = weight;

            Number = count;
            count++;
        }

        public int Number { get; }
        private static int count = 1;

        #region Поля
        public int Price;
        public double Rate;
        public int WorkTime;
        public int Sensitivity;
        public int Weight;
        #endregion

        public bool CompareTo(Headphones param)
        {
            if (Price <= param.Price &&
                Rate >= param.Rate &&
                WorkTime >= param.WorkTime &&
                Sensitivity >= param.Sensitivity &&
                Weight <= param.Weight)
                return true;
            else
                return false;
        }
    }

    static class Program
    {
        public static Random _generator = new Random();
        public static bool canShowPareto = true;

        static void Main(string[] args)
        {
            const string path = @"C:\Users\Ilya Axenov\source\repos\ParetoGenerator\ParetoGenerator\inputData.txt";
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
                    canShowPareto = false;
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
                    if (count <= 0 || count > 10000)
                        count = 50;
                    headphones = GetRandomList(count);
                    break;
            }

            headphones.ShowList();

            Console.WriteLine("ПОПАРНОЕ СРАВНЕНИЕ ВСЕХ АЛЬТЕРНАТИВ.\n");
            List<Headphones> paretoList = headphones.PairComparison();
            paretoList.ShowList();

            Console.WriteLine("УКАЗАНИЕ ВЕРХНИХ \\ НИЖНИХ ГРАНИЦ КРИТЕРИЕВ.\n");
            List<Headphones> bordersList = paretoList.CriteriaBorders();
            bordersList.ShowList();

            Console.WriteLine("СУБОПТИМИЗАЦИЯ.\n");
            int mainCriteria = GetSuboptimizationCriteria();
            List<Headphones> suboptimization = paretoList.CriteriaBorders(mainCriteria);
            Console.WriteLine("Выбор альтернатив по критериям:\n");
            suboptimization.ShowList();
            if (suboptimization.Count > 1)
            {
                Console.WriteLine("Выбор по главному критерию:\n");
                suboptimization = suboptimization.SelectByCriteria(mainCriteria);
                suboptimization.ShowList();
            }

            Console.WriteLine("ЛЕКСИКОГРАФИЧЕСКАЯ ОПТИМИЗАЦИЯ.\n");
            List<Headphones> lexicography = paretoList.Lexicography();
            lexicography.ShowList();

            Console.ReadLine();
        }

        static List<Headphones> GetHeadphonesFromFile(string path)
        {
            List<Headphones> headphones = new List<Headphones>();

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

                    headphones.Add(new Headphones(int.Parse(data[0]), double.Parse(data[1]),
                                                  int.Parse(data[2]), int.Parse(data[3]), int.Parse(data[4])));
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при чтении данных из файла.\n" + ex.Message);
            }

            return headphones;
        }

        static List<Headphones> GetRandomList(int count)
        {
            List<Headphones> headphones = new List<Headphones>();

            int price = 0;
            double rate = 0.0;
            int workTime = 0;
            int sensitivity = 0;
            int weight = 0;

            for (int i = 0; i < count; i ++)
            {
                price = _generator.Next(3000, 12000);
                price = (int)(price / 100) * 100;

                rate = _generator.Next(2, 5);
                rate += _generator.NextDouble();
                if (rate > 5)
                    rate = 5;

                rate = Math.Round(rate, 1);

                workTime = _generator.Next(3, 15);

                sensitivity = _generator.Next(90, 121);

                weight = _generator.Next(8, 50);

                headphones.Add(new Headphones(price, rate, workTime, sensitivity, weight));
            }

            return headphones;
        }

        static void ShowList(this List<Headphones> headphones)
        {
            Console.WriteLine("№    price     rate      time      sense     weight");

            foreach (Headphones h in headphones)
            {
                Console.WriteLine("{0, -5}{1, -10}{2, -10}{3, -10}{4, -10}{5, -10}", h.Number, h.Price, h.Rate, h.WorkTime, h.Sensitivity, h.Weight);
            }

            Console.WriteLine();
        }

        static bool IsConsist(this List<Headphones> headphones, int number)
        {
            foreach (Headphones h in headphones)
                if (h.Number == number)
                    return true;
            return false;
        }

        static List<Headphones> PairComparison(this List<Headphones> headphones)
        {
            string[,] array = new string[headphones.Count, headphones.Count];
            List<int> result = new List<int>();
            List<Headphones> ret = new List<Headphones>();

            for (int i = 0; i < headphones.Count; i++)
                for (int j = 0; j < headphones.Count; j++)
                    array[i, j] = "x";

            for (int i = 1; i < headphones.Count; i++)
                for (int j = 0; j < i; j++)
                    if (headphones[i].CompareTo(headphones[j]))
                        array[i, j] = (i + 1).ToString();
                    else if (headphones[j].CompareTo(headphones[i]))
                        array[i, j] = (j + 1).ToString();
                    else
                        array[i, j] = "н";

            if (canShowPareto)
                Console.WriteLine("   1   2   3   4   5   6   7   8   9");

            for (int i = 0; i < headphones.Count; i++)
                for (int j = 0; j < headphones.Count; j++)
                {
                    if (canShowPareto)
                    {
                        if (j == 0) Console.Write((i + 1).ToString() + "  ");
                        Console.Write(j == 8 ? array[i, j] + "\n" : array[i, j] + "   ");
                    }

                    if (Char.IsDigit(array[i, j], 0) && !result.Contains(int.Parse(array[i, j]) - 1))
                        result.Add(int.Parse(array[i, j]) - 1);
                }

            Console.WriteLine();

            result.Sort();

            foreach (int r in result)
                ret.Add(headphones[r]);

            return ret;
        }

        static List<Headphones> CriteriaBorders(this List<Headphones> headphones, int excludedCriteria = 0)
        {
            int maxPrice = 7000;
            double minRate = 4.2;
            int minWorkTime = 6;
            int minSensitivity = 98;
            int maxWeight = 20;

            string choice = "";

            if (excludedCriteria != 1)
            {
                Console.Write("Задайте значения границ критериев (или введите def, чтобы пропустить критерий):\n\nМаксимальная цена: ");
                choice = Console.ReadLine();
                try
                {
                    maxPrice = choice == "def" ? 1000000000 : int.Parse(choice);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Неправильный формат входных данных.\n" + ex.Message);
                    maxPrice = 1000000000;
                }
            }
            if (excludedCriteria != 2)
            {
                Console.Write("Минимальный рейтинг: ");
                choice = Console.ReadLine();
                try
                {
                    minRate = choice == "def" ? 0.0 : double.Parse(choice.Replace(".", ","));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Неправильный формат входных данных.\n" + ex.Message);
                    minRate = 0.0;
                }
            }
            if (excludedCriteria != 3)
            {
                Console.Write("Минимальное время работы: ");
                choice = Console.ReadLine();
                try
                {
                    minWorkTime = choice == "def" ? 0 : int.Parse(choice);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Неправильный формат входных данных.\n" + ex.Message);
                    minWorkTime = 0;
                }
            }
            if (excludedCriteria != 4)
            {
                Console.Write("Минимальная чувствительность: ");
                choice = Console.ReadLine();
                try
                {
                    minSensitivity = choice == "def" ? 0 : int.Parse(choice);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Неправильный формат входных данных.\n" + ex.Message);
                    minSensitivity = 0;
                }
            }
            if (excludedCriteria != 5)
            {
                Console.Write("Максимальный вес: ");
                choice = Console.ReadLine();
                try
                {
                    maxWeight = choice == "def" ? 1000000000 : int.Parse(choice);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Неправильный формат входных данных.\n" + ex.Message);
                    maxWeight = 1000000000;
                }
            }

            Console.WriteLine();

            var groupedList = headphones.Where(p => p.Price <= maxPrice).                                         
                                         Where(r => r.Rate  >= minRate).
                                         Where(t => t.WorkTime >= minWorkTime).
                                         Where(s => s.Sensitivity >= minSensitivity).
                                         Where(w => w.Weight <= maxWeight);

            List<Headphones> typedGroupedList = new List<Headphones>();

            foreach (var gl in groupedList)
            {
                typedGroupedList.Add((Headphones)gl);
            }

            return typedGroupedList;
        }

        static int GetSuboptimizationCriteria()
        {
            Console.Write("Укажите главный критерий (1 - цена, 2 - рейтинг, " +
                          "3 - время работы, 4 - чувствительность, 5 - максимальный вес):\n>> ");

            int choice = 0;

            while (choice != 1 && choice != 2 && choice != 3 && choice != 4 && choice != 5)
            {
                try
                {
                    choice = Convert.ToInt32(Console.ReadLine());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Неправильный формат входных данных.\n" + ex.Message);
                }
            }

            Console.WriteLine();

            return choice;
        }

        static List<Headphones> SelectByCriteria(this List<Headphones> headphones, int suboptimizationCriteria)
        {
            List<Headphones> result = new List<Headphones>();

            int index = 0;

            for (int i = 1; i < headphones.Count; i ++)
            {
                switch (suboptimizationCriteria)
                {
                    case 1:
                        if (headphones[index].Price > headphones[i].Price)
                            index = i;
                        break;
                    case 2:
                        if (headphones[index].Rate < headphones[i].Rate)
                            index = i;
                        break;
                    case 3:
                        if (headphones[index].WorkTime < headphones[i].WorkTime)
                            index = i;
                        break;
                    case 4:
                        if (headphones[index].Sensitivity < headphones[i].Sensitivity)
                            index = i;
                        break;
                    case 5:
                        if (headphones[index].Weight > headphones[i].Weight)
                            index = i;
                        break;
                    default:
                        if (headphones[index].Price > headphones[i].Price)
                            index = i;
                        break;
                }
            }

            for (int i = 0; i < headphones.Count; i ++)
            {
                switch (suboptimizationCriteria)
                {
                    case 1:
                        if (headphones[i].Price == headphones[index].Price)
                            result.Add(headphones[i]);
                        break;
                    case 2:
                        if (headphones[i].Rate == headphones[index].Rate)
                            result.Add(headphones[i]);
                        break;
                    case 3:
                        if (headphones[i].WorkTime == headphones[index].WorkTime)
                            result.Add(headphones[i]);
                        break;
                    case 4:
                        if (headphones[i].Sensitivity == headphones[index].Sensitivity)
                            result.Add(headphones[i]);
                        break;
                    case 5:
                        if (headphones[i].Weight == headphones[index].Weight)
                            result.Add(headphones[i]);
                        break;
                    default:
                        if (headphones[i].Price == headphones[index].Price)
                            result.Add(headphones[i]);
                        break;
                }
            }

            return result;
        }

        static List<Headphones> Lexicography(this List<Headphones> headphones)
        {
            List<Headphones> result = headphones;

            List<int> criterias = new List<int>() { 1, 2, 3, 4, 5 };

            int criteria;
            
            while (criterias != null)
            {
                criteria = GetSuboptimizationCriteria();

                if (!criterias.Contains(criteria))
                {
                    Console.WriteLine("Критерий уже был использован.\n");
                    continue;
                }
                else
                {
                    criterias.Remove(criteria);
                }

                result = result.SelectByCriteria(criteria);

                if (result.Count <= 1)
                    return result;
                else
                    result.ShowList();

                Console.WriteLine("Нажмите ESC, чтобы закончить оптимизацию, или ENTER, чтобы продолжить.\n");
                while (true)
                {
                    ConsoleKey key = Console.ReadKey().Key;
                    if (key == ConsoleKey.Escape)
                        return result;
                    else if (key == ConsoleKey.Enter)
                        break;
                }
            }

            return result;
        }
    }
}
