// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Text.Json;

namespace SCL.CommandLine.Extensions
{
    /// <summary>
    /// Extension to handle "set" commands
    /// </summary>
    public static class CommandSet
    {
        // list of valid keys
        private static List<string> ValidKeys => new ()
        {
            "Namespace",
            "AppName",
            "Args",
            "Port",
            "NodePort",
        };

        /// <summary>
        /// Extension method to add the app commands
        ///   this example uses Arguments as positional command line values
        /// </summary>
        /// <param name="parent">System.CommandLine.Command</param>
        public static void AddSetCommand(this Command parent)
        {
            // create the argument for the key
            Argument<string> key = new ("key");
            key.Description = $"Value to set ({string.Join(' ', ValidKeys)})";
            key.AddValidator(ValidateSetKey);

            // create the value for the value
            // note this is List<string> so multiple values can be passed
            // Args takes multiple values
            // Port and NodePort take integer values
            Argument<List<string>> value = new ("value");
            value.Description = "New value(s)";

            // create the set command
            Command set = new ("set", "Set application values");
            set.AddArgument(key);
            set.AddArgument(value);
            set.AddValidator(ValidateSet);
            set.Handler = CommandHandler.Create<AppSetConfig>(DoAppSetCommand);
            parent.AddCommand(set);
        }

        // validate app set command
        private static string ValidateSet(CommandResult result)
        {
            // return non-empty string to display an error
            string msg = string.Empty;

            return msg;
        }

        // validate combinations of parameters
        private static string ValidateSetKey(ArgumentResult result)
        {
            // return non-empty string to display an error
            string msg = string.Empty;

            // get the name of the argument
            string name = result.GetValueOrDefault<string>();

            // validate the key
            if (string.IsNullOrWhiteSpace(name))
            {
                msg += "key argument cannot be empty\n";
            }
            else
            {
                if (!ValidKeys.Contains(name))
                {
                    msg += $"invalid key\n  valid keys: {string.Join(' ', ValidKeys).Trim()}";
                }
            }

            // return error message(s) or string.empty
            return msg;
        }

        /// <summary>
        /// app set Command Handler
        /// </summary>
        /// <param name="config">parsed command line in AppSetConfig</param>
        /// <returns>0 on success</returns>
        private static int DoAppSetCommand(AppSetConfig config)
        {
            // sample implementation
            if (config.DryRun)
            {
                Console.WriteLine("Set Command");
                Console.WriteLine(JsonSerializer.Serialize<AppSetConfig>(config, AppConfig.JsonOptions));

                return 0;
            }

            Console.WriteLine("Set Command");
            Console.WriteLine(JsonSerializer.Serialize<AppSetConfig>(config, AppConfig.JsonOptions));

            return 0;
        }
    }
}
