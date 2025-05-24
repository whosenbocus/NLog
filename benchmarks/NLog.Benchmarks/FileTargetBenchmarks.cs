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

        [Benchmark]
        public void WriteLogEvent_KeepFileOpenFalse()
        {
            var config = new LoggingConfiguration();
            var target = new PublicFileTarget
            {
                FileName = LogFileName,
                Layout = "${message}",
                KeepFileOpen = false,
                AutoFlush = true
            };
            config.AddRuleForAllLevels(target);
            LogManager.Configuration = config;
            var evt = new LogEventInfo(LogLevel.Info, "BenchmarkLogger", "Test message");
            target.PublicWrite(evt);
        }

        [Benchmark]
        public void WriteLogEvent_AutoFlushFalse()
        {
            var config = new LoggingConfiguration();
            var target = new PublicFileTarget
            {
                FileName = LogFileName,
                Layout = "${message}",
                KeepFileOpen = true,
                AutoFlush = false
            };
            config.AddRuleForAllLevels(target);
            LogManager.Configuration = config;
            var evt = new LogEventInfo(LogLevel.Info, "BenchmarkLogger", "Test message");
            target.PublicWrite(evt);
        }

        [Benchmark]
        public void WriteLogEvent_ReplaceFileContentsOnEachWrite()
        {
            var config = new LoggingConfiguration();
            var target = new PublicFileTarget
            {
                FileName = LogFileName,
                Layout = "${message}",
                KeepFileOpen = true,
                AutoFlush = true,
                ReplaceFileContentsOnEachWrite = true
            };
            config.AddRuleForAllLevels(target);
            LogManager.Configuration = config;
            var evt = new LogEventInfo(LogLevel.Info, "BenchmarkLogger", "Test message");
            target.PublicWrite(evt);
        }

        [Benchmark]
        public void WriteLogEvent_ComplexLayout()
        {
            var config = new LoggingConfiguration();
            var target = new PublicFileTarget
            {
                FileName = LogFileName,
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}",
                KeepFileOpen = true,
                AutoFlush = true
            };
            config.AddRuleForAllLevels(target);
            LogManager.Configuration = config;
            var evt = new LogEventInfo(LogLevel.Info, "BenchmarkLogger", "Test message with more details");
            target.PublicWrite(evt);
        }

        [Benchmark]
        public void WriteLogEvent_Batch()
        {
            var config = new LoggingConfiguration();
            var target = new PublicFileTarget
            {
                FileName = LogFileName,
                Layout = "${message}",
                KeepFileOpen = true,
                AutoFlush = true
            };
            config.AddRuleForAllLevels(target);
            LogManager.Configuration = config;
            var events = new LogEventInfo[100];
            for (int i = 0; i < events.Length; i++)
                events[i] = new LogEventInfo(LogLevel.Info, "BenchmarkLogger", $"Batch message {i}");
            foreach (var evt in events)
                target.PublicWrite(evt);
        }
    }
}
