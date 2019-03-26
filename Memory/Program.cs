using System;
using BenchmarkDotNet.Running;

namespace Memory
{
    class Program
    {
        private static long _memoryStart;

        static void Main(string[] args)
        {
            TestClass testClass = new TestClass();

            // Using performance counter get available memory.
            var ramCounter = new System.Diagnostics.PerformanceCounter("Memory", "Available MBytes");
            Console.WriteLine($"Memory Available MBytes: {ramCounter.NextValue()}");

            // Test objects allocation.
            testClass.AllocObjectInMemory();

            // Test of MemoryStream.
            testClass.AllocMemory();

            // Test unmanaged memory allocation.
            testClass.AllocUnmanagedMemory();

            // Run metrics
            Console.WriteLine("Get metrics");
            BenchmarkRunner.Run<TestClass>();
            Console.ReadLine();
        }

        
    }
}
