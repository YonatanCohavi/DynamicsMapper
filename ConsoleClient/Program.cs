using Domain.Models;
using DynamicsMapper.Mappers;
using Microsoft.Xrm.Sdk;
using System.Data;

namespace ConsoleClient;
partial class Program
{
    static void Main(string[] args)
    {
        //CID: b695becf-c844-40db-b04f-92dd99bea4fb
        var contact = new Contact();
        var mapper = new ContactMapper();
    }

    public static T MapTo<T>(Entity entity)
    {
        var mapper2 = (IEntityMapper<T>) (typeof(T).Name switch
        {
            "123" => new ContactMapper(),
            _ => throw new Exception(),
        });
        return mapper2.Map(entity);
    }
}
