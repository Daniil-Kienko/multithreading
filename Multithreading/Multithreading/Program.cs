using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using EquationSolver;

class Program
{

    private static object locker = new object();

    const int EQUATIONS_COUNT = 1000;

    private static async void CreateEquationData(string fileName)
    {
        try
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < EQUATIONS_COUNT; i++)
            {
                int a = random.Next(50);
                int b = random.Next(50);
                int c = random.Next(50);
                builder.Append($"{a} {b} {c}\n");
            }
            using (StreamWriter sw = new StreamWriter(fileName))
                sw.WriteLine(builder);
            Console.WriteLine("Запись выполнена");
        }
        catch (Exception e) { Console.WriteLine(e.Message); }
    }

    private static List<Equation> ReadEquationData(string fileName)
    {
        List<Equation> res = new List<Equation>();
        try
        {
            using (StreamReader sr = new StreamReader(fileName))
                for (int i = 0; i < EQUATIONS_COUNT; i++)
                {
                    var data = sr.ReadLine().Split(" ");
                    var equation = new Equation();
                    equation.A = Double.Parse(data[0]);
                    equation.B = Double.Parse(data[1]);
                    equation.C = Double.Parse(data[2]);
                    res.Add(equation);
                }
            Console.WriteLine("Чтение выполнено");
        }
        catch (Exception e) { Console.WriteLine(e.Message); }
        return res;
    }

    private static List<Task<EqSolution>> CalculateEquations(List<Equation> equations)
    {
        EqSolver solver = new EqSolver();
        List<Task<EqSolution>> tasks = new List<Task<EqSolution>>();
        foreach (Equation equation in equations)
        {
            Task<EqSolution> task = new Task<EqSolution>(() => solver.ResolveEquation(equation).Result);
            tasks.Add(task);
            task.Start();
        }
        Task.WaitAll(tasks.ToArray());
        return tasks;
    }

    private static void WriteEquationResult(List<Task<EqSolution>> equationResults, string fileName)
    {
        try
        {
            foreach (var equation in equationResults)
                lock (locker)
                    using (StreamWriter sw = new StreamWriter(fileName, true))
                        sw.WriteLineAsync(equation.Result.Explanation);
        }
        catch (Exception e) { Console.WriteLine(e.Message); }
    }

    public static void PrintFuncExecutionTime(Action func)
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        func();
        watch.Stop();
        Console.WriteLine($"Elapsed {watch.ElapsedMilliseconds / 1000} seconds");
    }

    public static void Main()
    {
        PrintFuncExecutionTime(async () => {
            CreateEquationData("input.txt");
            List<Equation> equations = ReadEquationData("input.txt");
            List<Task<EqSolution>> explanationResults = CalculateEquations(equations);
            WriteEquationResult(explanationResults, "output.txt");
        });
    }
}