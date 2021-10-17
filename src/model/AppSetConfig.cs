// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SCL
{
    /// <summary>
    /// Model for setting app values
    /// System.CommandLine will parse and pass to the handler
    /// </summary>
    public class AppSetConfig : AppConfig
    {
        /// <summary>
        /// Gets or sets a value indicating the key for the new value
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the new value for the command
        /// </summary>
        public List<string> Value { get; set; }
    }
}
