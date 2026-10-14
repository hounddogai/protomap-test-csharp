namespace Acme.Workforce.Assets.Tests.LicenseSeat.Milestone;

using Acme.Workforce.Assets.LicenseSeat.Milestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LicenseSeatMilestoneEngineChecks
{
    public static void LicenseSeatMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatMilestoneEngine.DefaultLicenseSeatMilestoneEngine(Flags.NewRegistry());
        var item = LicenseSeatMilestone.NewLicenseSeatMilestone("code-751", "label-619");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatMilestoneEngineKeepsResultsWithinBounds();
    }
}
