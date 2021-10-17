// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Reflection;
using SCL.CommandLine.Extensions;

namespace SCL
{
    /// <summary>
    /// Main application class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">Command Line Parameters</param>
        /// <returns>0 on success</returns>
        public static int Main(string[] args)
        {
            // build the command line args
            RootCommand root = BuildRootCommand();

            // an alternate approach to using the ParseResult is to build a
            // middleware handler and inject into the pipeline before the default help handler

            // we all need ascii art :)
            DisplayAsciiArt(root.Parse(args));

            // invoke the correct command handler
            // once you understand what this one line of code does, it's really cool!
            // we add a command handler for each of the leaf commands and this automatically calls that handler
            // no switch or if statements!
            // allows for super clean code with no parsing!
            return root.Invoke(args);
        }

        // Build the RootCommand using System.CommandLine
        private static RootCommand BuildRootCommand()
        {
            // this is displayed in the help message
            RootCommand root = new ()
            {
                Name = "scl",
                Description = "System.CommandLine Sample App",
                TreatUnmatchedTokensAsErrors = true,
            };

            // we use extensions to build each command which makes reuse and reorg really fast and easy
            // notice there is no help or version command added
            // --help -h -? and --version are "automatic"
            // --version is controlled by the semver in the project
            //   versionprefix and versionsuffix

            // add the command handlers
            root.AddAddCommand();
            root.AddBootstrapCommand();
            root.AddBuildCommand();
            root.AddCheckCommand();
            root.AddConfigCommand();
            root.AddInitCommand();
            root.AddLogsCommand();
            root.AddRemoveCommand();
            root.AddSetCommand();
            root.AddSyncCommand();

            // add the global options
            // these options are available to all commands and sub commands
            // see AddBootstrapCommand for additional options on specific commands
            root.AddGlobalOption(new Option<bool>(new string[] { "--dry-run", "-d" }, "Validates and displays configuration"));
            root.AddGlobalOption(new Option<bool>(new string[] { "--verbose", "-v" }, "Show verbose output"));

            return root;
        }

        // display Ascii Art
        private static void DisplayAsciiArt(ParseResult parseResult)
        {
            // --version and --help will be parsed as unmatched tokens
            // we use ParseResult extensions to check for the unmatched tokens
            // the default System.CommandLine middleware will handle the unmatched tokens on invoke

            // don't display if --version
            if (!parseResult.HasVersionOption())
            {
                // display for help or dry-run
                if (parseResult.HasHelpOption() || parseResult.HasDryRunOption())
                {
                    // you have to get the path for this to work as a dotnet tool
                    string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string file = $"{path}/files/ascii-art.txt";

                    try
                    {
                        if (File.Exists(file))
                        {
                            string txt = File.ReadAllText(file);

                            if (!string.IsNullOrWhiteSpace(txt))
                            {
                                // GEAUX Tigers!
                                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                                Console.WriteLine(txt);
                            }
                        }
                    }
                    catch
                    {
                        // ignore any errors
                    }
                    finally
                    {
                        // reset the console
                        Console.ResetColor();
                    }
                }
            }
        }
    }
}
