namespace Acme.Workforce.Assets.Tests.Laptop.Ledger;

using Acme.Workforce.Assets.Laptop.Ledger;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LaptopLedgerEngineChecks
{
    public static void LaptopLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopLedgerEngine.DefaultLaptopLedgerEngine(Flags.NewRegistry());
        var item = LaptopLedger.NewLaptopLedger("code-693", "label-997");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopLedgerEngineKeepsResultsWithinBounds();
    }
}
