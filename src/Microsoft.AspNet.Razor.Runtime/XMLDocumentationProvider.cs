// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if !DNXCORE50 // Not used in coreclr
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Razor.Runtime
{
    public class XmlDocumentationProvider
    {
        private readonly IEnumerable<XElement> _members;

        public XmlDocumentationProvider(string xmlFileLocation)
        {
            // XML file processing is defined by: https://msdn.microsoft.com/en-us/library/fsbx0t7x.aspx
            var xmlDocumentation = XDocument.Load(xmlFileLocation);
            var documentationRootMembers = xmlDocumentation.Root.Element("members");
            _members = documentationRootMembers.Elements("member");
        }

        public string GetSummary(string id)
        {
            var associatedMemeber = GetMember(id);

            return associatedMemeber?.Element("summary")?.Value.Trim();
        }

        public string GetRemarks(string id)
        {
            var associatedMemeber = GetMember(id);

            return associatedMemeber?.Element("remarks")?.Value.Trim();
        }

        public bool HasDocumentation(string id)
        {
            return GetMember(id) != null;
        }

        public static string GetId([NotNull] TypeInfo typeInfo)
        {
            return $"T:{typeInfo.FullName}";
        }

        public static string GetId([NotNull] PropertyInfo propertyInfo)
        {
            var declaringTypeInfo = propertyInfo.DeclaringType.GetTypeInfo();
            return $"P:{declaringTypeInfo.FullName}.{propertyInfo.Name}";
        }

        private XElement GetMember(string id)
        {
            var associatedMemeber = _members
                .FirstOrDefault(element =>
                    string.Equals(element.Attribute("name").Value, id, StringComparison.Ordinal));

            return associatedMemeber;
        }
    }
}
#endif