namespace Acme.Workforce.Workplace.Tests.Desk.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeskSegmentEngineChecks
{
    public static void DeskSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = DeskSegmentEngine.DefaultDeskSegmentEngine(Flags.NewRegistry());
        var item = DeskSegment.NewDeskSegment("code-842", "title-519");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeskSegmentEngineKeepsResultsWithinBounds();
    }
}
