using Microsoft.CodeAnalysis;

namespace Generator
{
    public static class DemoDiagnosticsDescriptors
    {
        public static readonly DiagnosticDescriptor ClassMustBePartial = new(
            "YCRM001",
           "Class must be partial",
           "The class {0}' must be partial when using CrmEntity Attribute",
           "DemoAnalyzer",
           DiagnosticSeverity.Error,
           true);

        public static readonly DiagnosticDescriptor MultipleAttributes = new(
            "YCRM002",
           "Multiple Attributes are used {1}",
           "Multiple Attributes are used",
           "DemoAnalyzer",
           DiagnosticSeverity.Error,
           true);
    }
}
