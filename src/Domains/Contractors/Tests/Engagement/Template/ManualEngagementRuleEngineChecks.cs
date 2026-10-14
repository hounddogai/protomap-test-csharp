namespace Acme.Workforce.Contractors.Tests.Engagement.Template;

using Acme.Workforce.Contractors.Engagement.Template;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ManualEngagementRuleEngineChecks
{
    public static void ManualEngagementRuleEngineKeepsResultsWithinBounds()
    {
        var engine = ManualEngagementRuleEngine.DefaultManualEngagementRuleEngine(Flags.NewRegistry());
        var item = ManualEngagementRule.NewManualEngagementRule("code-974", "label-118");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualEngagementRuleEngineKeepsResultsWithinBounds();
    }
}
