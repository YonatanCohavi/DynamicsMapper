// See https://aka.ms/new-console-template for more information
using Microsoft.Xrm.Sdk;

Console.WriteLine("Hello, World!");
var x = new OptionSetValueCollection(Enumerable.Range(0,1).Select(i => new OptionSetValue(i)).ToList());
int[]? y;
