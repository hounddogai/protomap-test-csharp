namespace Acme.Workforce.Workplace.Tests.Desk.DeskProposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>Checks of desk proposal code, run by the project's runner.</summary>
public static class CappedDeskAdjustmentEngineChecks
{
    public static void CappedDeskAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = CappedDeskAdjustmentEngine.DefaultCappedDeskAdjustmentEngine(Flags.NewRegistry());
        var item = CappedDeskAdjustment.NewCappedDeskAdjustment("code-893", "label-499");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedDeskAdjustmentEngineKeepsResultsWithinBounds();
    }
}
