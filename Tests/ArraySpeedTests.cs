using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PISilnik.Tests
{
    public static class ArraySpeedTests
    {
        private const int testNum = 10000000; // int.MaxValue/10

        public static void TestArraySpeeds(int trials)
        {
            Debug.WriteLine("GC Collection...");

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            int[] ArrayTest = new int[testNum];
            List<int> ListTest = new();

            Debug.WriteLine($"Starting trials for {testNum} integers...");

            for (int i = 0; i < testNum; i++)
            {
                ArrayTest[i] = 1;
                ListTest.Add(1);
            }

            // with length index
            List<long> Results1 = new();
            List<long> Results2 = new();
            List<long> Results3 = new();
            List<long> Results4 = new();

            // with cached length
            List<long> Results5 = new();
            List<long> Results6 = new();

            int length = ArrayTest.Length;
            int count = ListTest.Count;

            for (int trial = 0; trial < trials; trial++)
            {
                Debug.WriteLine($"Collecting results for trial {trial + 1} / {trials}...");

                {
                    int check = 0;
                    Stopwatch watch = Stopwatch.StartNew();
                    foreach (int i in ArrayTest)
                        check += i;
                    watch.Stop();
                    Results2.Add(watch.ElapsedMilliseconds);
                }

                {
                    int check = 0;
                    Stopwatch watch = Stopwatch.StartNew();
                    for (int i = 0; i < ArrayTest.Length; i++)
                        check += ArrayTest[i];
                    watch.Stop();
                    Results1.Add(watch.ElapsedMilliseconds);
                }

                {
                    int check = 0;
                    Stopwatch watch = Stopwatch.StartNew();
                    for (int i = 0; i < ListTest.Count; i++)
                        check += ListTest[i];
                    watch.Stop();
                    Results3.Add(watch.ElapsedMilliseconds);
                }

                {
                    int check = 0;
                    Stopwatch watch = Stopwatch.StartNew();
                    foreach (int i in ListTest)
                        check += i;
                    watch.Stop();
                    Results4.Add(watch.ElapsedMilliseconds);
                }


                {
                    int check = 0;
                    Stopwatch watch = Stopwatch.StartNew();
                    for (int i = 0; i < length; i++)
                        check += ArrayTest[i];
                    watch.Stop();
                    Results5.Add(watch.ElapsedMilliseconds);
                }

                {
                    int check = 0;
                    Stopwatch watch = Stopwatch.StartNew();
                    for (int i = 0; i < count; i++)
                        check += ListTest[i];
                    watch.Stop();
                    Results6.Add(watch.ElapsedMilliseconds);
                }
            }

            Debug.WriteLine($"{Environment.NewLine}Average results for {testNum} integers ({trials} trials performed):{Environment.NewLine}");
            Debug.WriteLine($"Array/for: {(float)Results1.Sum() / (float)Results1.Count:0.00}ms average");
            Debug.WriteLine($"Array/for (cached length): {(float)Results5.Sum() / (float)Results5.Count:0.00}ms average");
            Debug.WriteLine($"Array/foreach: {(float)Results2.Sum() / (float)Results2.Count:0.00}ms average{Environment.NewLine}");
            Debug.WriteLine($"List/for: {(float)Results3.Sum() / (float)Results3.Count:0.00}ms average");
            Debug.WriteLine($"List/for (cached count): {(float)Results6.Sum() / (float)Results6.Count:0.00}ms average");
            Debug.WriteLine($"List/foreach: {(float)Results4.Sum() / (float)Results4.Count:0.00}ms average");
            Debug.WriteLine($"{Environment.NewLine}------------------------------------------");
        }
    }
}
