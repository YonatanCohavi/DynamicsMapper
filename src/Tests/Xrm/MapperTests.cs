#if NET8_0_OR_GREATER
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Xrm;
[TestClass]
public class MapperTests : FakeXrmEasyTestsBase
{
    Entity _contact;
    Entity _account;
    public MapperTests()
    {
        _contact = new Entity("contact", Guid.NewGuid());
        _account = new Entity("account", Guid.NewGuid());
    }

    [TestMethod]
    public void Test()
    {
        _context.Initialize([_contact, _account]);
        var q = new QueryExpression("account") { ColumnSet = new ColumnSet(true) };
        var accounts = _service.RetrieveMultiple(q).Entities;
    }
}
#endif