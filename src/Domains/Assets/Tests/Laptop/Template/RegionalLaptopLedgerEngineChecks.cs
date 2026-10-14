namespace Acme.Workforce.Assets.Tests.Laptop.Template;

using Acme.Workforce.Assets.Laptop.Template;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class RegionalLaptopLedgerEngineChecks
{
    public static void RegionalLaptopLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalLaptopLedgerEngine.DefaultRegionalLaptopLedgerEngine(Flags.NewRegistry());
        var item = RegionalLaptopLedger.NewRegionalLaptopLedger("code-593", "title-669");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalLaptopLedgerEngineKeepsResultsWithinBounds();
    }
}
