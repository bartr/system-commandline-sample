// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;

namespace SCL.CommandLine.Extensions
{
    /// <summary>
    /// Extension methods to add commands to system.commandline.command
    ///   Called from BuildRootCommand
    ///   AddBootstrapCommand is the most complex
    /// </summary>
    public static class CommandAdders
    {
        /// <summary>
        /// Extension method to add the add command
        ///   We use this as an example of the EnvVarOptions extension which reads options from env vars as well as command line
        /// </summary>
        /// <param name="parent">System.CommandLine.Command</param>
        public static void AddAddCommand(this Command parent)
        {
            Command c = new ("add", "example of using environment variables as default values");
            parent.AddCommand(c);

            // note we are using UserConfig so we can pick up the option we add below
            c.Handler = CommandHandler.Create<UserConfig>(CommandHandlers.DoAddCommand);

            // loading from env var using the EnvVarOptions extension
            //   this will pickup the user from the env vars
            //     you can override by specifying on the command line
            //   by using aliases, we can support all 3 options
            //     --user works for Linux / Mac env var
            //     --username works for Windows env var
            //     -u is the short option for convenience
            // imagine the code you didn't have to write ...
            c.AddOption(EnvVarOptions.AddOption(new string[] { "--user", "--username", "-u" }, "User name", string.Empty));
        }

        /// <summary>
        /// Extension method to add the bootstrap command
        ///   This is an example of command specific options and validation
        /// </summary>
        /// <param name="parent">System.CommandLine.Command</param>
        public static void AddBootstrapCommand(this Command parent)
        {
            // no handler because it's not a "leaf" command
            Command bs = new ("bootstrap", "example of using sub-command specific options and validation");

            // alias because it's easier to type
            bs.AddAlias("bs");

            // this is a leaf command
            Command add = new ("add", "example of using sub-command specific options and validation");
            add.Handler = CommandHandler.Create<BootstrapConfig>(CommandHandlers.DoBootstrapAddCommand);

            // these options will only be available to this command
            // we could add as global options to the parent command
            // I chose to do it this way to get the custom description
            // Note that our handler uses a different model for the command
            add.AddOption(new Option<List<string>>(new string[] { "--services", "-s" }, "bootstrap service(s) to add"));
            add.AddOption(new Option<bool>(new string[] { "--all", "-a" }, "Add all bootstrap services"));

            // command validator to make sure -s or -a (but not both) are provided
            add.AddValidator(ValidateBootstrapCommand);

            // same as add
            Command rm = new ("remove", "example of using sub-command specific options and validation");
            rm.AddAlias("rm");
            rm.Handler = CommandHandler.Create<BootstrapConfig>(CommandHandlers.DoBootstrapRemoveCommand);
            rm.AddOption(new Option<List<string>>(new string[] { "--services", "-s" }, "bootstrap service(s) to remove"));
            rm.AddOption(new Option<bool>(new string[] { "--all", "-a" }, "Remove all bootstrap services"));
            rm.AddValidator(ValidateBootstrapCommand);

            // add the commands to the tree
            bs.AddCommand(add);
            bs.AddCommand(rm);
            parent.AddCommand(bs);
        }

        /// <summary>
        /// Extension method to add the build command
        ///   Example using an enum and default value as an option
        /// </summary>
        /// <param name="parent">System.CommandLine.Command</param>
        public static void AddBuildCommand(this Command parent)
        {
            Command c = new ("build", "example using an enum option with defaults");
            c.Handler = CommandHandler.Create<BuildConfig>(CommandHandlers.DoBuildCommand);
            parent.AddCommand(c);

            // add an option for the BuildType enumeration
            // BuildType.Debug is the default
            c.AddOption(new Option<BuildType>(new string[] { "--build-type", "-b" }, () => BuildType.Debug, "Build type"));
        }

        // validate the bootstrap command parameters
        private static string ValidateBootstrapCommand(CommandResult result)
        {
            string msg = string.Empty;

            try
            {
                // get the values to validate
                List<string> services = result.Children.FirstOrDefault(c => c.Symbol.Name == "services") is OptionResult svcRes ? svcRes.GetValueOrDefault<List<string>>() : null;
                bool all = result.Children.FirstOrDefault(c => c.Symbol.Name == "all") is OptionResult allRes && allRes.GetValueOrDefault<bool>();

                if (services != null && services.Count == 0)
                {
                    services = null;
                }

                if (!all && services == null)
                {
                    msg += "--services or --all must be specified";
                }

                if (all && services != null)
                {
                    msg += "--services and --all cannot be combined";
                }
            }
            catch
            {
                // system.commandline will catch and display parse exceptions
            }

            // return error message(s) or string.empty
            return msg;
        }
    }
}
