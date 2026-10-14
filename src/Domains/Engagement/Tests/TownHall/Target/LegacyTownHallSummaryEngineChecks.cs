namespace Acme.Workforce.Engagement.Tests.TownHall.Target;

using Acme.Workforce.Engagement.TownHall.Target;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LegacyTownHallSummaryEngineChecks
{
    public static void LegacyTownHallSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyTownHallSummaryEngine.DefaultLegacyTownHallSummaryEngine(Flags.NewRegistry());
        var item = LegacyTownHallSummary.NewLegacyTownHallSummary("code-633", "label-517");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyTownHallSummaryEngineKeepsResultsWithinBounds();
    }
}
