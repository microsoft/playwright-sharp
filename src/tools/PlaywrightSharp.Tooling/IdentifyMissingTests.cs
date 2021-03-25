/*
 * MIT License
 *
 * Copyright (c) Microsoft Corporation.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PlaywrightSharp.Xunit;

namespace PlaywrightSharp.Tooling
{
    /// <summary>
    /// This will identify missing tests from upstream.
    /// </summary>
    internal static class IdentifyMissingTests
    {
        private static readonly List<PlaywrightTestAttribute> _testPairs = new();

        /// <summary>
        /// Runs the scenario.
        /// </summary>
        /// <param name="options">The options argument.</param>
        public static void Run(IdentifyMissingTestsOptions options)
        {
            // get all files that match a pattern
            var directoryInfo = new DirectoryInfo(options.SpecFileLocations);
            if (!directoryInfo.Exists)
            {
                throw new ArgumentException($"The location ({directoryInfo.FullName}) specified does not exist.");
            }

            // let's map the test cases from the spec files
            MapTestsCases(directoryInfo, options, string.Empty);

            // now, let's load the DLL and use some reflection-fu
            var assembly = Assembly.LoadFrom(options.TestsAssemblyPath);

            var attributes = assembly.DefinedTypes.SelectMany(
                type => type.GetMethods().SelectMany(method => method.GetCustomAttributes<PlaywrightTestAttribute>()));

            int potentialMatches = 0;
            int fullMatches = 0;
            int noMatches = 0;
            int totalTests = 0;

            List<PlaywrightTestAttribute> missingTests = new();
            List<KeyValuePair<PlaywrightTestAttribute, List<PlaywrightTestAttribute>>> invalidMaps = new();
            foreach (var atx in attributes)
            {
                totalTests++;

                // a test can either be a full match, a partial (i.e. just the test name) or no match
                var potentialMatch = _testPairs.Where(x => string.Equals(x.TestName, atx.TestName, StringComparison.InvariantCultureIgnoreCase));
                if (!potentialMatch.Any())
                {
                    noMatches++;
                    missingTests.Add(atx);
                }
                else if (potentialMatch.Any(x => string.Equals(x.TrimmedName, atx.TrimmedName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    fullMatches++;
                    continue;
                }
                else
                {
                    invalidMaps.Add(new KeyValuePair<PlaywrightTestAttribute, List<PlaywrightTestAttribute>>(atx, potentialMatch.ToList()));
                    potentialMatches++;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Total matching tests: {fullMatches}/{totalTests}.");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Total tests found by name, but not by file: {potentialMatches}/{totalTests}.");
            Console.ResetColor();

            foreach (var invalidTest in invalidMaps)
            {
                Console.WriteLine($"{invalidTest.Key}");
                foreach (var value in invalidTest.Value)
                {
                    Console.WriteLine($"\t{value}");
                }
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Total missing tests: {noMatches}/{totalTests}.");
            Console.ResetColor();

            foreach (var invalidTest in missingTests)
            {
                Console.WriteLine($"{invalidTest}");
            }

            Console.WriteLine($"Found/Mismatched/Missing: {fullMatches}/{potentialMatches}/{noMatches} out of {totalTests}");
        }

        private static void MapTestsCases(DirectoryInfo directoryInfo, IdentifyMissingTestsOptions options, string basePath)
        {
            // get the sub-directories
            if (options.Recursive)
            {
                foreach (var subdirectory in directoryInfo.GetDirectories())
                {
                    MapTestsCases(subdirectory, options, $"{basePath}{subdirectory.Name}/");
                }
            }

            foreach (var fileInfo in directoryInfo.GetFiles(options.Pattern))
            {
                ScaffoldTest.FindTestsInFile(
                    fileInfo.FullName,
                    (testDescribe, testName) =>
                    {
                        _testPairs.Add(new PlaywrightTestAttribute(basePath + fileInfo.Name, testDescribe, testName));
                    });
            }
        }
    }
}
