namespace Test.Automated
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Test.Shared;
    using Touchstone.Cli;
    using Touchstone.Core;

    /// <summary>
    /// Touchstone console runner for the OpenAuditLog shared suites.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Run all shared Touchstone suites, optionally filtered by suite and/or test name.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>Process exit code; zero when every executed test passes.</returns>
        public static async Task<int> Main(string[] args)
        {
            if (IsHelpRequested(args))
            {
                ShowHelp();
                return 0;
            }

            string suiteFilter = ReadOption(args, "suite");
            string testFilter = ReadOption(args, "test");

            IReadOnlyList<TestSuiteDescriptor> suites = FilterSuites(OpenAuditLogSuites.All, suiteFilter, testFilter);

            if (!String.IsNullOrWhiteSpace(suiteFilter) || !String.IsNullOrWhiteSpace(testFilter))
            {
                Console.WriteLine("Test filter: suite=" + (suiteFilter ?? "*") + " test=" + (testFilter ?? "*"));
                Console.WriteLine();
            }

            return await ConsoleRunner.RunAsync(suites).ConfigureAwait(false);
        }

        private static bool IsHelpRequested(string[] args)
        {
            if (args == null) return false;
            foreach (string argument in args)
            {
                string normalized = (argument ?? String.Empty).Trim().ToLowerInvariant();
                if (normalized == "--help" || normalized == "-h" || normalized == "-?") return true;
            }
            return false;
        }

        private static void ShowHelp()
        {
            Console.WriteLine("OpenAuditLog Touchstone runner");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run --project src/Test.Automated/Test.Automated.csproj");
            Console.WriteLine("  dotnet run --project src/Test.Automated/Test.Automated.csproj -- --suite emitter");
            Console.WriteLine("  dotnet run --project src/Test.Automated/Test.Automated.csproj -- --test round_trip");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --suite <text>   Only run suites whose id contains <text> (case-insensitive).");
            Console.WriteLine("  --test <text>    Only run tests whose id contains <text> (case-insensitive).");
            Console.WriteLine("  --help           Show this help.");
        }

        private static string ReadOption(string[] args, string name)
        {
            if (args == null) return null;

            for (int i = 0; i < args.Length; i++)
            {
                string inlineValue;
                string normalized = Normalize(args[i] ?? String.Empty, out inlineValue);
                if (normalized == name)
                {
                    if (inlineValue != null) return inlineValue;
                    if (i + 1 < args.Length) return args[i + 1];
                    return String.Empty;
                }
            }

            return null;
        }

        private static IReadOnlyList<TestSuiteDescriptor> FilterSuites(
            IReadOnlyList<TestSuiteDescriptor> suites,
            string suiteFilter,
            string testFilter)
        {
            if (suites == null) throw new ArgumentNullException(nameof(suites));

            List<TestSuiteDescriptor> filtered = new List<TestSuiteDescriptor>();
            foreach (TestSuiteDescriptor suite in suites)
            {
                if (!Matches(suite.SuiteId, suiteFilter)) continue;

                List<TestCaseDescriptor> cases = suite.Cases
                    .Where(testCase => Matches(testCase.CaseId, testFilter))
                    .ToList();
                if (cases.Count == 0) continue;

                filtered.Add(new TestSuiteDescriptor(
                    suite.SuiteId,
                    suite.DisplayName,
                    cases,
                    suite.BeforeSuiteAsync,
                    suite.AfterSuiteAsync));
            }

            return filtered;
        }

        private static bool Matches(string value, string filter)
        {
            if (String.IsNullOrWhiteSpace(filter)) return true;
            return value != null && value.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string Normalize(string argument, out string inlineValue)
        {
            inlineValue = null;
            if (String.IsNullOrWhiteSpace(argument)) return String.Empty;

            string normalized = argument.Trim();
            while (normalized.StartsWith("-", StringComparison.Ordinal))
            {
                normalized = normalized.Substring(1);
            }

            int equalsIndex = normalized.IndexOf('=');
            if (equalsIndex >= 0)
            {
                inlineValue = normalized.Substring(equalsIndex + 1);
                normalized = normalized.Substring(0, equalsIndex);
            }

            return normalized.ToLowerInvariant();
        }
    }
}
