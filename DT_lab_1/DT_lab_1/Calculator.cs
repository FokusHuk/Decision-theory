using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DT_lab_1
{
    public static class Calculator
    {
        public static double calc(string expression)
        {
            return Convert.ToDouble(CalculatePolishExpression(GetPolishNotation(expression)));
        }

        // Возвращает арифметическое выражение, переведенное в обратную польскую запись
        private static string GetPolishNotation(string expression)
        {
            // Удаление пробелов из конвертируемого выражения
            expression = expression.Replace(" ", "");

            // Обработка операторов вида +=
            if (expression.Contains("=") && IsOperator(expression[expression.IndexOf("=") - 1]))
            {
                expression = expression[0] + "=" + expression[0] + expression[1] + "(" + expression.Substring(3) + ")";
            }

            // Выражение, переведенное в польскую запись
            string convertedExpression = "";

            // Объявление стека и установка начальных параметров выражения для перевода в обратную польскую нотацию
            Stack<char> stack = new Stack<char>();

            stack.Push('(');

            expression += ")";

            int i = 0;

            while (stack.Count != 0)
            {
                // Текущий символ выражения - число d=1+2
                if (Char.IsDigit(expression[i]) || Char.IsLetter(expression[i]) || expression[i] == '?' || expression[i] == ',')
                {
                    convertedExpression += expression[i];
                }
                // Текущий символ выражения - открывающая скобка
                else if (expression[i] == '(')
                {
                    stack.Push('(');
                }
                // Текущий символ выражения - оператор
                else if (expression[i].IsOperator())
                {
                    if (convertedExpression.Length != 0)
                        convertedExpression += " ";

                    // Пока верхний элемент стека содержит операцию и 
                    // ее приоритет меньше приоритета операции, прочитанной из выражения, 
                    // извлекать операции из стека в convertedExpression
                    while (stack.Peek().IsOperator() && CompareOperators(expression[i], stack.Peek()) && stack.Peek() != '=')
                    {
                        convertedExpression += stack.Pop() + " ";
                    }

                    if (expression[i] == '-')
                    {
                        if (i == 0)
                        {
                            convertedExpression += "0 ";
                        }
                        else if (expression[i - 1] == '(')
                        {
                            convertedExpression += "0 ";
                        }
                    }

                    stack.Push(expression[i]);
                }
                // Текущий символ выражения - закрывающая скобка
                if (expression[i] == ')')
                {
                    // Пока на вершине стека не окажется открывающая скобка, 
                    // извлекать из стека элементы и записывать их в выходную строку
                    while (stack.Peek() != '(')
                    {
                        convertedExpression += " ";
                        convertedExpression += stack.Pop();
                    }
                    // Из стека извлекается открывающая скобка
                    stack.Pop();
                }
                i++;
            }

            return convertedExpression;
        }

        // Возвращает результат арифметического выражения, представленного в обратной польской записи
        private static string CalculatePolishExpression(string polishExpression)
        {
            Stack<string> stack = new Stack<string>();

            string number = "";

            for (int i = 0; i < polishExpression.Length; i++)
            {
                // Текущий символ выражения - число
                if (Char.IsDigit(polishExpression[i]) || Char.IsLetter(polishExpression[i]))
                {
                    // Если число многозначное, то оно посимвольно считывается в буфер number
                    // А затем конвертируется в int и помещается в стек
                    while (Char.IsDigit(polishExpression[i]) || polishExpression[i] == ',')
                    {
                        number += polishExpression[i];
                        i++;

                        if (i >= polishExpression.Length)
                        {
                            return number;
                        }
                    }

                    stack.Push(number);

                    number = "";
                }
                // Текущий символ выражения - оператор            
                else if (polishExpression[i].IsOperator())
                {
                    double expResult = 0;
                    // Из стека извлекаются два числа и над ними производится действие оператора
                    if (polishExpression[i] == '~')
                    {
                        expResult = Calculate(polishExpression[i], Convert.ToDouble(stack.Pop()));

                        // Результат вычисления помещается в стек
                        stack.Push(expResult.ToString());
                    }
                    else
                    {
                        string arg1 = stack.Pop();
                        string arg2 = stack.Pop();

                        expResult = Calculate(polishExpression[i], Convert.ToDouble(arg1), Convert.ToDouble(arg2));

                        // Результат вычисления помещается в стек
                        stack.Push(expResult.ToString());
                    }
                }
            }

            return stack.Pop();
        }


        // Функция, вычисляющая выражение [arg2] [operator] [arg1]
        // Обратный порядок аргументов используется из-за обратного порядка взятия этих аргументов из стека
        private static double Calculate(char op, double arg1, double arg2 = 1)
        {
            switch (op)
            {
                case '+': return arg2 + arg1;
                case '-': return arg2 - arg1;
                case '*': return arg2 * arg1;
                case '/': return arg2 / arg1;
                case '%': return arg2 % arg1;
                case '~': return -arg1;
                default: return 1;
            }
        }

        // Функция сравнения приоритетов двух операторов
        private static bool CompareOperators(char operator_1, char operator_2)
        {
            switch (operator_1)
            {
                case '=': operator_1 = '1'; break;
                case '+': operator_1 = '2'; break;
                case '-': operator_1 = '2'; break;
                case '*': operator_1 = '5'; break;
                case '/': operator_1 = '4'; break;
                case '%': operator_1 = '3'; break;
                case '~': operator_1 = '6'; break;
            }

            switch (operator_2)
            {
                case '=': operator_2 = '1'; break;
                case '+': operator_2 = '2'; break;
                case '-': operator_2 = '2'; break;
                case '*': operator_2 = '5'; break;
                case '/': operator_2 = '4'; break;
                case '%': operator_2 = '3'; break;
                case '~': operator_2 = '6'; break;
            }

            if ((int)operator_1 <= (int)operator_2)
            {
                return true;
            }

            return false;
        }

        // Функция-расширение, определяющая, является ли данный символ арифметическим оператором
        private static bool IsOperator(this char c)
        {
            if (c == '+' || c == '-' || c == '*' || c == '/' || c == '%' || c == '~' || c == '=')
                return true;
            return false;
        }

    }
}
