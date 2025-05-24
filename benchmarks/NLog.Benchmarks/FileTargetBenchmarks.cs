using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using NLog.Targets;
using NLog;
using NLog.Config;

namespace NLog.Benchmarks
{
    public class PublicFileTarget : FileTarget
    {
        public void PublicWrite(LogEventInfo logEvent) => base.Write(logEvent);
    }

    [SimpleJob(RuntimeMoniker.Net60)]
    [SimpleJob(RuntimeMoniker.Net80)]
    [SimpleJob(RuntimeMoniker.Net462)]
    [RPlotExporter]
    public class FileTargetBenchmarks
    {
        private const string LogFileName = "benchmark.log";
        private PublicFileTarget fileTarget;
        private LogEventInfo logEvent;

        [GlobalSetup]
        public void Setup()
        {
            // Clean up log file before benchmark
            if (File.Exists(LogFileName))
                File.Delete(LogFileName);

            var config = new LoggingConfiguration();
            fileTarget = new PublicFileTarget
            {
                FileName = LogFileName,
                Layout = "${message}", // Simple layout for benchmarking
                KeepFileOpen = true,    // For best performance
                AutoFlush = true
            };
            config.AddRuleForAllLevels(fileTarget);
            LogManager.Configuration = config; // This initializes the target
            logEvent = new LogEventInfo(LogLevel.Info, "BenchmarkLogger", "Test message");
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            // Clean up log file after benchmark
            if (File.Exists(LogFileName))
                File.Delete(LogFileName);
        }

        [Benchmark]
        public void WriteLogEvent()
        {
            fileTarget.PublicWrite(logEvent);
        }
    }
}
