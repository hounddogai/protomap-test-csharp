namespace Acme.Workforce.Compensation.Tests.PayBand.Milestone;

using Acme.Workforce.Compensation.PayBand.Milestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PayBandMilestoneEngineChecks
{
    public static void PayBandMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandMilestoneEngine.DefaultPayBandMilestoneEngine(Flags.NewRegistry());
        var item = PayBandMilestone.NewPayBandMilestone("code-715", "title-723");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandMilestoneEngineKeepsResultsWithinBounds();
    }
}
