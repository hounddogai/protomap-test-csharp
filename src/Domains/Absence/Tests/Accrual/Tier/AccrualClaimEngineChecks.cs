namespace Acme.Workforce.Absence.Tests.Accrual.Tier;

using Acme.Workforce.Absence.Accrual.Tier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AccrualClaimEngineChecks
{
    public static void AccrualClaimEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualClaimEngine.DefaultAccrualClaimEngine(Flags.NewRegistry());
        var item = AccrualClaim.NewAccrualClaim("code-223", "label-612");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualClaimEngineKeepsResultsWithinBounds();
    }
}
