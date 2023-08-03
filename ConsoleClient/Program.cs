using Domain.Models;

namespace ConsoleClient;
partial class Program
{
    static void Main(string[] args)
    {
        var contact = new Contact();
        var entity = contact.ToEntity();
    }
}
