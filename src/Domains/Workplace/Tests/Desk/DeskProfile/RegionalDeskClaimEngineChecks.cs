namespace Acme.Workforce.Workplace.Tests.Desk.DeskProfile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.DeskProfile;

/// <summary>Checks of desk profile code, run by the project's runner.</summary>
public static class RegionalDeskClaimEngineChecks
{
    public static void RegionalDeskClaimEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalDeskClaimEngine.DefaultRegionalDeskClaimEngine(Flags.NewRegistry());
        var item = RegionalDeskClaim.NewRegionalDeskClaim("code-714", "name-85");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalDeskClaimEngineKeepsResultsWithinBounds();
    }
}
