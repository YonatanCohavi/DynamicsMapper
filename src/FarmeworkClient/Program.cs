using Domain.Models;
using Microsoft.Xrm.Sdk;
using System;

namespace FarmeworkClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IOrganizationService service;
            service.Retrieve("contact", Guid.NewGuid(), Contact.ColumnSet);
            var contact = new Contact();
            var entity = contact.ToEntity();
        }
    }
}
