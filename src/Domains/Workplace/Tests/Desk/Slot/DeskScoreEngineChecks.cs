namespace Acme.Workforce.Workplace.Tests.Desk.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeskScoreEngineChecks
{
    public static void DeskScoreEngineKeepsResultsWithinBounds()
    {
        var engine = DeskScoreEngine.DefaultDeskScoreEngine(Flags.NewRegistry());
        var item = DeskScore.NewDeskScore("code-769", "title-861");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeskScoreEngineKeepsResultsWithinBounds();
    }
}
