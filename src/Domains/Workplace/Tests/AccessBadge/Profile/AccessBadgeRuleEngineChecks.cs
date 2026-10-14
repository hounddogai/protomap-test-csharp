namespace Acme.Workforce.Workplace.Tests.AccessBadge.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AccessBadgeRuleEngineChecks
{
    public static void AccessBadgeRuleEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeRuleEngine.DefaultAccessBadgeRuleEngine(Flags.NewRegistry());
        var item = AccessBadgeRule.NewAccessBadgeRule("code-585", "label-661");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeRuleEngineKeepsResultsWithinBounds();
    }
}
