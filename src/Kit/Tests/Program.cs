namespace Acme.Workforce.Kit.Tests;

using Acme.Workforce.Kit.Tests.Cache;
using Acme.Workforce.Kit.Tests.Collections;
using Acme.Workforce.Kit.Tests.Flags;
using Acme.Workforce.Kit.Tests.Ids;
using Acme.Workforce.Kit.Tests.Money;
using Acme.Workforce.Kit.Tests.Numbers;
using Acme.Workforce.Kit.Tests.Retry;
using Acme.Workforce.Kit.Tests.Table;
using Acme.Workforce.Kit.Tests.Text;

/// <summary>Runs every check of the project and reports how many failed.</summary>
public static class Program
{
    public static int Main()
    {
        var checks = new List<(string Name, Action Run)>
        {
            ("TextChecks", TextChecks.RunAll),
            ("CollectionsChecks", CollectionsChecks.RunAll),
            ("NumbersChecks", NumbersChecks.RunAll),
            ("MoneyChecks", MoneyChecks.RunAll),
            ("CacheChecks", CacheChecks.RunAll),
            ("RetryChecks", RetryChecks.RunAll),
            ("FlagsChecks", FlagsChecks.RunAll),
            ("TableChecks", TableChecks.RunAll),
            ("IdsChecks", IdsChecks.RunAll),
        };
        var failed = 0;
        foreach (var (name, run) in checks)
        {
            try
            {
                run();
            }
            catch (InvalidOperationException error)
            {
                Console.Error.WriteLine($"{name}: {error.Message}");
                failed++;
            }
        }
        Console.WriteLine($"{checks.Count - failed} of {checks.Count} passed");
        return failed == 0 ? 0 : 1;
    }
}
