using BenchmarkDotNet.Running;

namespace NLog.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<FileTargetBenchmarks>();
        }
    }
}
