namespace Acme.Workforce.Contractors.Tests.Engagement.Target;

using Acme.Workforce.Contractors.Engagement.Target;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class EngagementBundleEngineChecks
{
    public static void EngagementBundleEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementBundleEngine.DefaultEngagementBundleEngine(Flags.NewRegistry());
        var item = EngagementBundle.NewEngagementBundle("code-274", "label-555");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementBundleEngineKeepsResultsWithinBounds();
    }
}
