namespace Acme.Workforce.Compensation.Tests.EquityGrant.Summary;

using Acme.Workforce.Compensation.EquityGrant.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class EquityGrantSummaryEngineChecks
{
    public static void EquityGrantSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantSummaryEngine.DefaultEquityGrantSummaryEngine(Flags.NewRegistry());
        var item = EquityGrantSummary.NewEquityGrantSummary("code-766", "label-159");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantSummaryEngineKeepsResultsWithinBounds();
    }
}
