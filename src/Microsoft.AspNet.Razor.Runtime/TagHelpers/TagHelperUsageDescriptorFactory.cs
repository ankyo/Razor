// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !DNXCORE50 // Cannot accurately resolve the location of the documentation XML file in coreclr.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    public static class TagHelperUsageDescriptorFactory
    {
        private static readonly char[] InvalidPathCharacters = Path.GetInvalidPathChars();

        public static TagHelperUsageDescriptor CreateDescriptor([NotNull] TypeInfo typeInfo)
        {
            var id = XmlDocumentationProvider.GetId(typeInfo);
            return CreateDescriptorCore(typeInfo.Assembly, id);
        }

        public static TagHelperUsageDescriptor CreateDescriptor([NotNull] PropertyInfo propertyInfo)
        {
            var id = XmlDocumentationProvider.GetId(propertyInfo);
            var declaringAssembly = propertyInfo.DeclaringType.GetTypeInfo().Assembly;
            return CreateDescriptorCore(declaringAssembly, id);
        }

        private static TagHelperUsageDescriptor CreateDescriptorCore(Assembly assembly, string id)
        {
            var assemblyLocation = assembly.Location;

            if (string.IsNullOrWhiteSpace(assemblyLocation) && !string.IsNullOrWhiteSpace(assembly.CodeBase))
            {
                var uri = new UriBuilder(assembly.CodeBase);

                // Normalize the path to a UNC path. This will remove things like file:// from start of the uri.Path.
                assemblyLocation = Uri.UnescapeDataString(uri.Path);

                // Still couldn't resolve a valid assemblyLocation.
                if (string.IsNullOrWhiteSpace(assemblyLocation))
                {
                    return null;
                }
            }

            XmlDocumentationProvider documentationProvider = null;
            var xmlDocumentationFile = GetXmlDocumentationFile(assemblyLocation);

            // We only want to process the file if it exists. In the case it doesn't, a null value will be added
            // to the cache to not constantly look for new XML files.
            if (xmlDocumentationFile != null)
            {
                documentationProvider = new XmlDocumentationProvider(xmlDocumentationFile.FullName);
            }

            // Members will be null if there is no associated XML file for the provided assembly.
            if (documentationProvider != null)
            {
                var summary = documentationProvider.GetSummary(id);
                var remarks = documentationProvider.GetRemarks(id);

                if (!string.IsNullOrEmpty(summary) || !string.IsNullOrEmpty(remarks))
                {
                    return new TagHelperUsageDescriptor(summary, remarks);
                }
            }

            return null;
        }

        private static FileInfo GetXmlDocumentationFile(string assemblyLocation)
        {
            if (string.IsNullOrWhiteSpace(assemblyLocation) ||
                assemblyLocation.IndexOfAny(InvalidPathCharacters) != -1)
            {
                return null;
            }

            try
            {
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
                var assemblyName = Path.GetFileName(assemblyLocation);
                var assemblyXmlDocumentationName = Path.ChangeExtension(assemblyName, ".xml");
                var culture = CultureInfo.CurrentCulture;

                var assemblyXmlDocumentationFile = new FileInfo(
                    Path.Combine(assemblyDirectory, assemblyXmlDocumentationName));

                // If there's not an XML file side-by-side the .dll it may exist in a culture specific directory.
                if (!assemblyXmlDocumentationFile.Exists)
                {
                    var fallbackDirectories = GetCultureFallbackDirectories();
                    assemblyXmlDocumentationFile = fallbackDirectories
                        .Select(fallbackDiretory =>
                            new FileInfo(
                                Path.Combine(assemblyDirectory, fallbackDiretory, assemblyXmlDocumentationName)))
                        .FirstOrDefault(file => file.Exists);
                }

                return assemblyXmlDocumentationFile;
            }
            catch (PathTooLongException)
            {
                // Could not resolve XML file.
                return null;
            }
        }

        private static IEnumerable<string> GetCultureFallbackDirectories()
        {
            var culture = CultureInfo.CurrentCulture;

            // Following the fall-back process defined by:
            // https://msdn.microsoft.com/en-us/library/sb6a8618.aspx#cpconpackagingdeployingresourcesanchor1
            do
            {
                yield return culture.Name;

                culture = culture.Parent;
            } while (culture != null && culture != CultureInfo.InvariantCulture);
        }
    }
}
#endif