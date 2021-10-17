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
    ///   This is an example of organizing options by class
    ///   This extension adds, validates and executes the set commands
    ///   This extension also uses Arguments as positional command line parameters
    /// </summary>
    public static class CommandSet
    {
        // list of valid keys
        //   the format of the command is scl key value [value2 value3 ...]
        //   we could use a sub-command for "key" instead of an argument
        //     we would create a handler per sub-command with that approach
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
        ///   this example uses Arguments as positional command line parameters
        /// </summary>
        /// <param name="parent">System.CommandLine.Command</param>
        public static void AddSetCommand(this Command parent)
        {
            // create the argument for the key
            // we could use a sub-command instead
            Argument<string> key = new ("key");
            key.Description = $"Value to set ({string.Join(' ', ValidKeys)})";
            key.AddValidator(ValidateSetKey);

            // create the value for the value
            // note this is List<string> so multiple values can be passed
            //   Args takes an array of strings
            //   Port and NodePort take integer values
            Argument<List<string>> value = new ("value");
            value.Description = "New value(s)";

            // create the set command
            Command set = new ("set", "Set application values");
            set.AddArgument(key);
            set.AddArgument(value);
            set.AddValidator(ValidateSet);
            set.Handler = CommandHandler.Create<AppSetConfig>(DoSetCommand);
            parent.AddCommand(set);
        }

        // validate combinations of arguments
        private static string ValidateSet(CommandResult result)
        {
            // return non-empty string to display an error
            string msg = string.Empty;

            try
            {
                ArgumentResult keyResult = (ArgumentResult)result.Children.GetByAlias("key");
                ArgumentResult valueResult = (ArgumentResult)result.Children.GetByAlias("value");

                if (keyResult == null || valueResult == null)
                {
                    return msg + "Failed to parse key value pair\n";
                }

                string key = keyResult.GetValueOrDefault<string>();

                if (string.IsNullOrWhiteSpace(key))
                {
                    msg += "Failed to parse key\n";
                }

                List<string> values = valueResult.GetValueOrDefault<List<string>>();

                if (values == null || values.Count == 0)
                {
                    msg += "Failed to parse value(s)\n";
                }

                // no parse errors
                if (string.IsNullOrEmpty(msg))
                {
                    switch (key)
                    {
                        case "Port":
                        case "NodePort":
                            msg += ValidatePort(key, values);
                            break;
                        case "Args":
                            // args takes an array of values so skip the default validation
                            break;
                        default:
                            // only one value
                            if (values.Count > 1)
                            {
                                msg += $"{key} only takes one value\n";
                            }

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                msg += $"Parsing exception: {ex.Message}\n";
            }

            return msg;
        }

        // validate Port and NodePort
        //   this is an example where it would be easier to have sub-commands vs. key Argument
        //     System.CommandLine would handle the int parsing by declaration
        private static string ValidatePort(string key, List<string> values)
        {
            string msg = string.Empty;

            // set min and max based on key
            int min = key == "Port" ? 1 : 30000;
            int max = key == "Port" ? 64 * 1024 : 32 * 1024;

            // only one value
            if (values.Count > 1)
            {
                msg += $"{key} only takes one value\n";
            }

            // must be int >= min and < max
            if (!int.TryParse(values[0], out int port) || port < min || port >= max)
            {
                msg += $"{key} must be an integer >= {min} and < {max}\n";
            }

            return msg;
        }

        // validate the key argument
        private static string ValidateSetKey(ArgumentResult result)
        {
            // return non-empty string to display an error
            string msg = string.Empty;

            // get the key from the argument
            string key = result.GetValueOrDefault<string>();

            // validate the key
            if (string.IsNullOrWhiteSpace(key))
            {
                msg += "key argument cannot be empty\n";
            }
            else
            {
                if (!ValidKeys.Contains(key))
                {
                    msg += $"invalid key\n  valid keys: {string.Join(' ', ValidKeys).Trim()}\n";
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
        private static int DoSetCommand(AppSetConfig config)
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
