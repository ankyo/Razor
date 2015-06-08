// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Razor.TagHelpers
{
    public class TagHelperUseageDescriptor
    {
        public TagHelperUseageDescriptor(string summary, string remarks)
        {
            Summary = summary;
            Remarks = remarks;
        }

        public string Summary { get; }

        public string Remarks { get; }
    }
}