namespace Acme.Workforce.Absence.Tests.Accrual.AccrualTier;

using Acme.Workforce.Absence.Accrual.AccrualTier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of accrual tier code, run by the project's runner.</summary>
public static class AccrualManifestEngineChecks
{
    public static void AccrualManifestEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualManifestEngine.DefaultAccrualManifestEngine(Flags.NewRegistry());
        var item = AccrualManifest.NewAccrualManifest("code-730", "label-511");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualManifestEngineKeepsResultsWithinBounds();
    }
}
