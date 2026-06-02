using System;
using System.Linq;
using System.Reflection;
using Wolverine;

class Program {
    static void Main() {
        var asm = typeof(WolverineOptions).Assembly;
        var types = asm.GetTypes().Where(t => t.Name.Contains("Envelope") || t.Name.Contains("Policy") || t.Name.Contains("Outgoing") || t.Name.Contains("Middleware"));
        foreach(var t in types) {
            Console.WriteLine(t.FullName);
        }
    }
}
