namespace Acme.Workforce.Absence.Tests.Balance.Milestone;

using Acme.Workforce.Absence.Balance.Milestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BalanceSegmentEngineChecks
{
    public static void BalanceSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = BalanceSegmentEngine.DefaultBalanceSegmentEngine(Flags.NewRegistry());
        var item = BalanceSegment.NewBalanceSegment("code-9", "title-346");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BalanceSegmentEngineKeepsResultsWithinBounds();
    }
}
