﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CommandLine;
using Humanizer;
using Newtonsoft.Json;
using Stubble.Core.Performance.Data;

namespace Stubble.Core.Performance
{
    public class Program
    {
        public static int Iterations = 10;
        public static readonly int[] Increments = {100, 1000, 10000, 100000, 1000000 };
        public static ProgramOptions Options;
        public static Stopwatch GlobalStopwatch;

        public static readonly List<OutputData> Outputs = new List<OutputData>
        {
            new OutputData("Stubble (Without Cache)", new Candidates.StubbleNoCache(), ConsoleColor.Yellow),
            new OutputData("Stubble (With Cache)", new Candidates.StubbleCache(), ConsoleColor.DarkYellow),
            new OutputData("Nustache", new Candidates.Nustache(), ConsoleColor.Cyan)
        };

        public static void Main(string[] args)
        {
            var parsed = CommandLine.Parser.Default.ParseArguments<ProgramOptions>(args);
            var errCode = parsed.MapResult(options =>
            {
                DoStuff(options);
                return 0;
            }, errs => 1);

            //return errCode;
        }

        public static void DoStuff(ProgramOptions options)
        {
            Options = options;
            GlobalStopwatch = Stopwatch.StartNew();

            DumpSettings(Options);

            if (!Options.ShowTitles && Options.ShouldLog)
            {
                foreach (var output in Outputs)
                {
                    ConsoleExtensions.WriteLineColor(output.OutputColor, output.Name);
                }
            }

            Iterations = Options.NumberOfIterations;
            for (var i = 1; i <= Iterations; i++)
            {
                ConsoleExtensions.WriteLine($"Iteration {i}".ToUpper());

                foreach (var increment in Increments)
                {
                    RunIncrement(increment);
                }
            }
            GlobalStopwatch.Stop();
            if (Options.ShouldOutput) WriteOutputs(DateTime.UtcNow);
            if (Options.ShouldHaltOnEnd) ConsoleExtensions.WriteLine("DONE");
            if (Options.ShouldHaltOnEnd) Console.ReadLine();
        }

        public static void DumpSettings(ProgramOptions options)
        {
            ConsoleExtensions.WriteLineColor(ConsoleColor.Green, "****** {0} ******", "CONFIGURATIONS");
            foreach (var prop in options.GetType().GetProperties())
            {
                ConsoleExtensions.WriteLine("{0} -> {1}", Regex.Replace(prop.Name, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1"), prop.GetValue(options, null));
            }
            ConsoleExtensions.WriteLineColor(ConsoleColor.Green, "****** {0} *****", new string('-', "CONFIGURATIONS".Length));
        }

        public static void RunIncrement(int increment)
        {
            foreach (var output in Outputs)
            {
                if (Options.ShouldLog && Options.ShowTitles) ConsoleExtensions.WriteLineColor(output.OutputColor, "****** {0} ******", output.Name.ToUpper());
                var timeElapsed = output.Candidate.RunTest(increment);
                output.AddIncrement(increment, timeElapsed);
                if (Options.ShouldLog) ConsoleExtensions.WriteLineColor(output.OutputColor, "Iteration {0:N0}\t: {1} ({2})", increment, timeElapsed.Humanize(), timeElapsed);
            }
        }

        public static void WriteOutputs(DateTime now)
        {
            var outputDir = $"./Perf/{now:dd-MM-yyyy}";
            CreateDirectoryIfNotExists(outputDir);
            WriteJson(outputDir, now);
            WriteOutputCsv(outputDir, now);
        }

        public static void WriteJson(string dir, DateTime now)
        {
            var serializer = new JsonSerializer {Formatting = Formatting.Indented};
            using (var sw = new StreamWriter($"{dir}/results-{now:H-mm-ss}.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, Outputs);
            }
        }

        public static void WriteOutputCsv(string dir, DateTime now)
        {
            using (var writer = new StreamWriter($"{dir}/results-{now:H-mm-ss}.csv"))
            {
                writer.WriteLine(string.Join(",", "Increment", string.Join(",", Outputs.Select(x => x.Name))));
                foreach (var increment in Increments)
                {
                    var incrementVal = increment;
                    writer.WriteLine(string.Join(",", incrementVal, string.Join(",", Outputs.Select(x => x.IncrementResultsAverage[incrementVal].ToString(CultureInfo.InvariantCulture)))));
                }
                writer.WriteLine(new string(',', Outputs.Count + 1));
                foreach (var increment in Increments)
                {
                    var incrementVal = increment;
                    writer.WriteLine(string.Join(",", incrementVal, string.Join(",", Outputs.Select(x => x.RelativeValues[incrementVal].ToString(CultureInfo.InvariantCulture)))));
                }
            }
        }

        public static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }

    public class ProgramOptions
    {
        [Option('s', "ShouldLog", Default = false, HelpText = "Should Log Output?")]
        public bool ShouldLog { get; set; }

        [Option('o', "ShouldOutput", Default = true, HelpText = "Should Output results?")]
        public bool ShouldOutput { get; set; }

        [Option('h', "ShouldHaltOnEnd", Default = false, HelpText = "Should Halt on End of Run?")]
        public bool ShouldHaltOnEnd { get; set; }

        [Option('t', "ShowTitles", Default = false, HelpText = "Should show titles?")]
        public bool ShowTitles { get; set; }

        [Option('i', "Iterations", Default = 10, HelpText = "Number of Iterations that should be run?")]
        public int NumberOfIterations { get; set; }
    }
}