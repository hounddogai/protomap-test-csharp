namespace Acme.Workforce.Workplace.Tests.AccessBadge.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccessBadgeReviewEngineChecks
{
    public static void AccessBadgeReviewEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeReviewEngine.DefaultAccessBadgeReviewEngine(Flags.NewRegistry());
        var item = AccessBadgeReview.NewAccessBadgeReview("code-286", "label-491");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeReviewEngineKeepsResultsWithinBounds();
    }
}
