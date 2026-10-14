namespace Acme.Workforce.Workplace.Tests.AccessBadge.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class WeightedAccessBadgeExceptionEngineChecks
{
    public static void WeightedAccessBadgeExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedAccessBadgeExceptionEngine.DefaultWeightedAccessBadgeExceptionEngine(Flags.NewRegistry());
        var item = WeightedAccessBadgeException.NewWeightedAccessBadgeException("code-345", "label-357");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedAccessBadgeExceptionEngineKeepsResultsWithinBounds();
    }
}
