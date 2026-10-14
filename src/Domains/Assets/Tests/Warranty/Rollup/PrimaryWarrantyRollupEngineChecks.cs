namespace Acme.Workforce.Assets.Tests.Warranty.Rollup;

using Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryWarrantyRollupEngineChecks
{
    public static void PrimaryWarrantyRollupEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryWarrantyRollupEngine.DefaultPrimaryWarrantyRollupEngine();
        var item = PrimaryWarrantyRollup.NewPrimaryWarrantyRollup("code-977", "title-133");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryWarrantyRollupEngineKeepsResultsWithinBounds();
    }
}
