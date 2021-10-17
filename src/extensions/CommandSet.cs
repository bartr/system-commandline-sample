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
        /// </summary>
        /// <param name="parent">System.CommandLine.Command</param>
        public static void AddSetCommand(this Command parent)
        {
            Argument<string> key = new ("key");
            key.Description = $"Value to set ({string.Join(' ', ValidKeys)})";
            key.AddValidator(ValidateSetKey);

            Argument<List<string>> value = new ("value");
            value.Description = "New value(s)";

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
            string msg = string.Empty;

            return msg;
        }

        // validate combinations of parameters
        private static string ValidateSetKey(ArgumentResult result)
        {
            string msg = string.Empty;

            string name = result.GetValueOrDefault<string>();

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
