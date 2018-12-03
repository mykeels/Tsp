using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MSearch;
using MSearch.GA;
using MSearch.SA;
using MSearch.Common;
using MSearch.Extensions;

namespace Tsp
{
    class Program
    {
        /** the number of consecutive elements that are selected from the first parent during crossover */
        const int NO_OF_CROSSOVER = 3;
        /** the actual problem */
        static List<(int x, int y)> data;

        static void Main(string[] args)
        {
            // read the problem
            data = ReadData("./data/1.txt");

            Configuration<Int16[]> config = GetConfiguration();

            GeneticAlgorithm<Int16[]> ga = new GeneticAlgorithm<Int16[]>((Int16[] sol1, Int16[] sol2) => {
                return Order1<Int16>(sol1.AsEnumerable(), sol2.AsEnumerable(), NO_OF_CROSSOVER).ToArray();
            });
            // seed the config
            ga.create(config);
            // run the iterations
            Int16[] sol = ga.fullIteration();
            // print out the result
            Console.WriteLine(String.Join(", ", sol) + " => " + config.objectiveFunction(sol));
        }

        static List<(int x, int y)> ReadData(string path) {
            List<(int x, int y)> ret = new List<(int x, int y)>();
            IEnumerable<string> lines = File.ReadLines(path);
            foreach (string line in lines) {
                string[] nums = line.Split(' ');
                (int x, int y) coord = (x: Convert.ToInt32(nums[1]), y: Convert.ToInt32(nums[2]));
                ret.Add(coord);
            }
            return ret;
        }

        /** generates the MSearch config */
        static Configuration<Int16[]> GetConfiguration()
        {
            Configuration<Int16[]> config = new Configuration<Int16[]>();
            config.cloneFunction = (Int16[] sol) => sol.ToList().ToArray();
            config.initializeSolutionFunction = () => {
                return Enumerable.Range(0, 15).Select(num => Convert.ToInt16(num)).ToArray();
            };
            config.movement = Search.Direction.Optimization;
            config.mutationFunction = (Int16[] sol) => {
                if (sol.Count() <= 1) throw new Exception("sol.Count should be greater than 1");
                int i = Convert.ToInt32(Math.Floor(Number.Rnd() * sol.Count()));
                int j = Convert.ToInt32(Math.Floor(Number.Rnd() * sol.Count()));
                while (i == j) {
                    j = Convert.ToInt32(Math.Floor(Number.Rnd() * sol.Count()));
                }
                Int16 third = sol[i];
                sol[i] = sol[j];
                sol[j] = third;
                return sol;
            };
            config.noOfIterations = 1500;
            config.objectiveFunction = (Int16[] sol) => {
                double distance = 0;
                if (sol.Count() == 0) return 0;
                (int x, int y) c1 = data[sol[0]];
                for (int i = 1; i < sol.Count(); i++) {
                    (int x, int y) c2 = data[sol[i]];
                    distance += Math.Sqrt(Math.Pow(c2.y - c1.y, 2) + Math.Pow(c2.x - c1.x, 2));
                    c1 = c2;
                }
                (int x, int y) first = data[sol[0]];
                distance += Math.Sqrt(Math.Pow(c1.y - first.y, 2) + Math.Pow(c1.x - first.x, 2));
                Console.WriteLine(distance);
                return distance;
            };
            config.populationSize = 30;
            config.selectionFunction = Selection.RoulleteWheel;
            config.writeToConsole = true;
            config.consoleWriteInterval = 100;
            config.enforceHardObjective = false;
            return config;
        }

        static IEnumerable<T> Order1<T>(IEnumerable<T> sol1, IEnumerable<T> sol2, int noOfCrossOver)
        {
            int sol1Count = sol1.Count(), sol2Count = sol2.Count();
            if (sol1 == null || sol2 == null) throw new Exception("None of the Arguments can be null");
            if (sol1Count != sol2Count) throw new Exception("Element Count in both Arguments must be the same");
            if (sol1Count <= noOfCrossOver) throw new Exception("No. of Elements to CrossOver must be less than Total Elements");
            T[] ret = new T[sol1Count];
            int startIndex = Convert.ToInt32(Math.Floor(Number.Rnd() * (sol1Count - noOfCrossOver)));
            int endIndex = startIndex + noOfCrossOver - 1;
            Dictionary<T, bool> record = new Dictionary<T, bool>();
            for (int i = startIndex; i <= endIndex; i++)
            {
                ret[i] = sol1.ElementAt(i);
                record.Add(ret[i], true);
            }
            for (int i = 0; i < sol2Count; i++)
            {
                if (!record.ContainsKey(sol2.ElementAt(i))) {
                    for (int j = 0; j < ret.Count(); j++) {
                        if (ret.ElementAt(j).Equals(default(T))) {
                            ret[j] = sol2.ElementAt(i);
                            record.Add(ret[j], true);
                            break;
                        }
                    }
                }
            }
            return ret.AsEnumerable();
        }
    }
}
