using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace Memory
{
    [MemoryDiagnoser] // we need to enable it in explicit way
    [RyuJitX64Job, LegacyJitX86Job] // let's run the benchmarks for 32 & 64 bit
    public class TestClass
    {
        public long MemoryStart { get; private set; }

        /// <summary>
        /// .ctor
        /// </summary>
        public TestClass()
        {
            // Get number of bytes for reallocation before test started.
            MemoryStart = GC.GetTotalMemory(false);
        }

        /// <summary>
        /// Method to test memory allocation for memory stream.
        /// </summary>
        public void AllocMemory()
        {
            Console.WriteLine("Staring memory allocation for MemoryStream...");
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    try
                    {
                        ms.WriteByte(1);
                    }
                    catch (OutOfMemoryException)
                    {
                        LogMetrics(ms.Length);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Method to test allocations for objects.
        /// </summary>
        [Benchmark]
        public void AllocObjectInMemory()
        {
            Console.WriteLine("Staring memory allocation for List<T>...");
            List<SomeClass> list = new List<SomeClass>();
            while (true)
            {
                try
                {
                    SomeClass sc = new SomeClass();
                    list.Add(sc);
                }
                catch (OutOfMemoryException)
                {
                    // Get diffs between initially reallocated memory and reallocated memory after test.
                    LogMetrics(GC.GetTotalMemory(false) - MemoryStart);
                    break;
                }
            }

            list.Clear();
            GC.Collect();
        }

        /// <summary>
        /// Method to test allocations for unmanaged memory
        /// </summary>
        public void AllocUnmanagedMemory()
        {
            Console.WriteLine("Staring unmanaged memory allocation...");
            int bufferChunkBytes = 1024 * 1024 * 1; //1MB
            int bufferSize = bufferChunkBytes * 1024; // 1GB

            while (true)
            {
                try
                {
                    IntPtr hGlobal = new IntPtr(bufferSize);
                    hGlobal = Marshal.AllocHGlobal(hGlobal);
                    Marshal.FreeHGlobal(hGlobal);
                }
                catch (OutOfMemoryException)
                {
                    LogMetrics(bufferSize);
                    break;
                }

                bufferSize += bufferChunkBytes;
            }
        }

        /// <summary>
        /// User friendly output.
        /// </summary>
        /// <param name="bufferSize">Buffer size.</param>
        static void LogMetrics(long bufferSize)
        {
            Console.WriteLine("Amount of available memory to allocate {0} bytes.", bufferSize);
            Console.WriteLine("Amount of available memory to allocate {0} KB.", bufferSize / 1024.0);
            Console.WriteLine("Amount of available memory to allocate {0} MB.", bufferSize / 1024.0 / 1024.0);
            Console.WriteLine("Amount of available memory to allocate {0} GB.", bufferSize / 1024.0 / 1024.0 / 1024.0);
        }
    }
}
