// See https://aka.ms/new-console-template for more information

using PowerSupplies.Core;
using PowerSupplies.Core.Hmp;
using PowerSupplies.Core.Psh;

Console.WriteLine("Hello, World!");


// создаем клиент




var hmp = new Hmp2020();


var r = Task.Run(() =>
{
    hmp.Connect("127.0.0.1:9098:COM1");
    Console.WriteLine("HMP 1: " + DateTime.Now);
    Console.WriteLine(hmp.Info);
    Console.WriteLine("HMP 2: " + DateTime.Now);
});


var psh = new Psh3610();
var r1 = Task.Run(() =>
{
    psh.Connect("127.0.0.1:9098:COM1");
    Console.WriteLine("PSH 1: " + DateTime.Now);
    Console.WriteLine(psh.Info);
    Console.WriteLine("PSH 2: " + DateTime.Now);
});


await r;
await r1;