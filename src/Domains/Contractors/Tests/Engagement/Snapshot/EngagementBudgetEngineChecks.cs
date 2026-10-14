namespace Acme.Workforce.Contractors.Tests.Engagement.Snapshot;

using Acme.Workforce.Contractors.Engagement.Snapshot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class EngagementBudgetEngineChecks
{
    public static void EngagementBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementBudgetEngine.DefaultEngagementBudgetEngine(Flags.NewRegistry());
        var item = EngagementBudget.NewEngagementBudget("code-954", "title-821");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementBudgetEngineKeepsResultsWithinBounds();
    }
}
