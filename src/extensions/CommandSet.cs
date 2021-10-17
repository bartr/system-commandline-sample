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
        //     with the Argument approach, we have to do some parsing and a switch for validation
        //     with the sub-command approach, you have one handler per leaf command
        //     the number and type of keys and the complexity of validation seem to drive this choice
        private static List<string> ValidKeys => new ()
        {
            "Namespace",
            "AppName",
            "Args",
            "Port",
            "NodePort",
        };

        /// <summary>
        /// Extension method to add the set commands
        ///   this example uses Arguments as positional command line parameters
        /// </summary>
        /// <param name="parent">System.CommandLine.Command</param>
        public static void AddSetCommand(this Command parent)
        {
            // create the argument for the key
            // we could use a sub-command instead
            Argument<string> key = new ("key");
            key.Description = $"Value to set ({string.Join(' ', ValidKeys)})";

            // create the value for the value
            // note this is List<string> so multiple values can be passed
            //   Args takes an array of strings
            //   Port and NodePort take integer values
            Argument<List<string>> value = new ("value");
            value.Description = "New value(s)";

            // create the set command
            Command set = new ("set", "example using positional Arguments with validation");
            set.AddArgument(key);
            set.AddArgument(value);
            set.AddValidator(ValidateSet);
            set.Handler = CommandHandler.Create<AppSetConfig>(DoSetCommand);
            parent.AddCommand(set);
        }

        // validate arguments
        private static string ValidateSet(CommandResult result)
        {
            // return non-empty string to display an error
            string msg = string.Empty;

            try
            {
                // get the results
                ArgumentResult keyResult = (ArgumentResult)result.Children.GetByAlias("key");
                ArgumentResult valueResult = (ArgumentResult)result.Children.GetByAlias("value");

                // let System.CommandLine handle this
                if (keyResult == null || valueResult == null)
                {
                    return msg;
                }

                string key = keyResult.GetValueOrDefault<string>();

                // validate the key - stop on error
                if (string.IsNullOrWhiteSpace(key))
                {
                    return msg + "key argument cannot be empty\n  valid keys: {string.Join(' ', ValidKeys).Trim()}\n";
                }
                else
                {
                    // case sensitive compare against list of valid keys
                    if (!ValidKeys.Contains(key))
                    {
                        return msg + $"invalid key\n  valid keys: {string.Join(' ', ValidKeys).Trim()}\n";
                    }
                }

                List<string> values = valueResult.GetValueOrDefault<List<string>>();

                // validate values - stop on error
                if (values == null || values.Count == 0)
                {
                    return msg + "Failed to parse value(s)\n";
                }

                // validate the value(s) based on key
                switch (key)
                {
                    case "Port":
                    case "NodePort":
                        // must be integer within range
                        return msg + ValidatePort(key, values);
                    case "Args":
                        // args takes an array of values so skip the default validation
                        break;
                    default:
                        // only one value passed
                        if (values.Count > 1)
                        {
                            return msg + $"{key} only takes one value\n";
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                return msg + $"Parsing exception: {ex.Message}\n";
            }

            return msg;
        }

        // validate Port and NodePort
        //   this is an example where it might be easier to have sub-commands vs. key Argument
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

        // set Command Handler
        private static int DoSetCommand(AppSetConfig config)
        {
            if (config.DryRun)
            {
                // handle --dry-run
            }

            // replace with your implementation
            Console.WriteLine("Set Command");
            Console.WriteLine(JsonSerializer.Serialize<AppSetConfig>(config, AppConfig.JsonOptions));

            return 0;
        }
    }
}
