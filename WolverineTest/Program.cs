using System;
using System.Linq;
using Wolverine;
using Wolverine.Configuration;

class Program {
    static void Main() {
        var e = typeof(Endpoint);
        Console.WriteLine(string.Join(", ", e.GetProperties().Select(p => p.Name).Distinct()));
    }
}
