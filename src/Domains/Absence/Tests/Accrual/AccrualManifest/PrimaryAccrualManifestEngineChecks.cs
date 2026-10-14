namespace Acme.Workforce.Absence.Tests.Accrual.AccrualManifest;

using Acme.Workforce.Absence.Accrual.AccrualManifest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of accrual manifest code, run by the project's runner.</summary>
public static class PrimaryAccrualManifestEngineChecks
{
    public static void PrimaryAccrualManifestEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryAccrualManifestEngine.DefaultPrimaryAccrualManifestEngine(Flags.NewRegistry());
        var item = PrimaryAccrualManifest.NewPrimaryAccrualManifest("code-572", "title-983");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryAccrualManifestEngineKeepsResultsWithinBounds();
    }
}
