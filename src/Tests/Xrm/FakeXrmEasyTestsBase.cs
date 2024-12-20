#if NET8_0_OR_GREATER
using FakeXrmEasy.Abstractions;
using FakeXrmEasy.Abstractions.Enums;
using FakeXrmEasy.Middleware;
using FakeXrmEasy.Middleware.Crud;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Tests.Xrm;
public class FakeXrmEasyTestsBase
{
    protected readonly IXrmFakedContext _context;
    protected readonly IOrganizationServiceAsync2 _service;

    public FakeXrmEasyTestsBase()
    {
        _context = MiddlewareBuilder
                        .New()
                        .AddCrud()
                        .UseCrud()

                        // Here we are saying we're using FakeXrmEasy (FXE) under a commercial context
                        // For more info please refer to the license at https://dynamicsvalue.github.io/fake-xrm-easy-docs/licensing/license/
                        // And the licensing FAQ at https://dynamicsvalue.github.io/fake-xrm-easy-docs/licensing/faq/
                        .SetLicense(FakeXrmEasyLicense.Commercial)
                        .Build();

        _service = _context.GetAsyncOrganizationService2();
    }
}


#endif