using Microsoft.CodeAnalysis;

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

    }
}
