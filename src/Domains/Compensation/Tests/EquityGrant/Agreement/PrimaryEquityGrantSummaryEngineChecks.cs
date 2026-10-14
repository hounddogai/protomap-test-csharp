namespace Acme.Workforce.Compensation.Tests.EquityGrant.Agreement;

using Acme.Workforce.Compensation.EquityGrant.Agreement;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class PrimaryEquityGrantSummaryEngineChecks
{
    public static void PrimaryEquityGrantSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryEquityGrantSummaryEngine.DefaultPrimaryEquityGrantSummaryEngine(Flags.NewRegistry());
        var item = PrimaryEquityGrantSummary.NewPrimaryEquityGrantSummary("code-262", "title-548");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryEquityGrantSummaryEngineKeepsResultsWithinBounds();
    }
}
