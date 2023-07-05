// See https://aka.ms/new-console-template for more information

using PowerSupplies.Core;

Console.WriteLine("Hello, World!");

Console.WriteLine("1");
var hmp = new Hmp2020();
Console.WriteLine("2");
hmp.Connect("COM7");
Console.WriteLine("3");