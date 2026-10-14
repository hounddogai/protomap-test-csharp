namespace Acme.Workforce.Assets.Tests.Refresh.Plan;

using Acme.Workforce.Assets.Refresh.Plan;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class LegacyRefreshSummaryEngineChecks
{
    public static void LegacyRefreshSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyRefreshSummaryEngine.DefaultLegacyRefreshSummaryEngine(Flags.NewRegistry());
        var item = LegacyRefreshSummary.NewLegacyRefreshSummary("code-172", "label-152");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyRefreshSummaryEngineKeepsResultsWithinBounds();
    }
}
