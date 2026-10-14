namespace Acme.Workforce.Contractors.Tests.Engagement.Tier;

using Acme.Workforce.Contractors.Engagement.Tier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RegionalEngagementBudgetEngineChecks
{
    public static void RegionalEngagementBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalEngagementBudgetEngine.DefaultRegionalEngagementBudgetEngine(Flags.NewRegistry());
        var item = RegionalEngagementBudget.NewRegionalEngagementBudget("code-227", "label-259");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalEngagementBudgetEngineKeepsResultsWithinBounds();
    }
}
