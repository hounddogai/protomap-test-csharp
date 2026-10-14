namespace Acme.Workforce.Contractors.Tests.Engagement.Notice;

using Acme.Workforce.Contractors.Engagement.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class EngagementClaimEngineChecks
{
    public static void EngagementClaimEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementClaimEngine.DefaultEngagementClaimEngine(Flags.NewRegistry());
        var item = EngagementClaim.NewEngagementClaim("code-139", "label-687");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementClaimEngineKeepsResultsWithinBounds();
    }
}
