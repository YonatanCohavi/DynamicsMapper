﻿using Microsoft.CodeAnalysis;

namespace DynamicsMapper
{
    public static class DiagnosticsDescriptors
    {
        public static readonly DiagnosticDescriptor NullableElementOnMultipleOptions = new(
           id: "DYM001",
           title: "Invalid Target Type",
           messageFormat: "Nullable elements are not supported for 'MultipleOptions' mapping",
           category: "DynamicsMapper",
           DiagnosticSeverity.Error,
           isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidType = new(
            id: "DYM002",
            title: "Invalid Target Type",
            messageFormat: "{0} is invalid target type for '{1}', the suppoirted types are: {2}",
            category: "DynamicsMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor NoLogicalname = new(
           id: "DYM003",
           title: "Logical name is not defined",
           messageFormat: "Logical name is not defined for {0}",
           category: "DynamicsMapper",
           DiagnosticSeverity.Error,
           isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor NoTarget = new(
           id: "DYM004",
           title: "No target",
           messageFormat: "{0} has no target, target is mandatory for lookups",
           category: "DynamicsMapper",
           DiagnosticSeverity.Error,
           isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MultiplePrimaryIds = new(
           id: "DYM005",
           title: "Multiple Primary ids",
           messageFormat: "{0} has multiple 'PrimaryId' mappings",
           category: "DynamicsMapper",
           DiagnosticSeverity.Error,
           isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor NotPartial = new(
         id: "DYM006",
         title: "Class Is Not Partial",
         messageFormat: "CrmEntity decorated class must be partial",
         category: "DynamicsMapper",
         DiagnosticSeverity.Error,
         isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor DuplicateSchemas = new(
            id: "DYM007",
            title: "Dulplicate schama names",
            messageFormat: "{0} has other attributes with the schema name \"{1}\"",
            category: "DynamicsMapper",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor DestinationMapperNotFound = new(
            id: "DYM008",
            title: "Destination mapper not found",
            messageFormat: "Mapper not found for the '{0}'",
            category: "DynamicsMapper",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor DuplicateMappings = new(
          id: "DYM009",
          title: "Dulplicate mapping",
          messageFormat: "{0} has other attributes with the schema name \"{1}\" using the \"{2}\" mapping type",
          category: "DynamicsMapper",
          DiagnosticSeverity.Error,
          isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor LinkMustBeUsedOnClass = new(
           id: "DYM010",
           title: "CrmLinkAttribute must be used on an object",
           messageFormat: "CrmLinkAttribute must be used on a class",
           category: "DynamicsMapper",
           DiagnosticSeverity.Error,
           isEnabledByDefault: true);
    }
}
