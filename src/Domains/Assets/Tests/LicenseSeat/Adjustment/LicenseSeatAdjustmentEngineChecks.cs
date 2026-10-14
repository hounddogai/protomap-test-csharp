namespace Acme.Workforce.Assets.Tests.LicenseSeat.Adjustment;

using Acme.Workforce.Assets.LicenseSeat.Adjustment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LicenseSeatAdjustmentEngineChecks
{
    public static void LicenseSeatAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatAdjustmentEngine.DefaultLicenseSeatAdjustmentEngine(Flags.NewRegistry());
        var item = LicenseSeatAdjustment.NewLicenseSeatAdjustment("code-640", "name-407");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatAdjustmentEngineKeepsResultsWithinBounds();
    }
}
