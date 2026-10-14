namespace Acme.Workforce.Absence.Tests.LeaveRequest.Item;

using Acme.Workforce.Absence.LeaveRequest.Item;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class TieredLeaveRequestItemEngineChecks
{
    public static void TieredLeaveRequestItemEngineKeepsResultsWithinBounds()
    {
        var engine = TieredLeaveRequestItemEngine.DefaultTieredLeaveRequestItemEngine(Flags.NewRegistry());
        var item = TieredLeaveRequestItem.NewTieredLeaveRequestItem("code-580", "title-220");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredLeaveRequestItemEngineKeepsResultsWithinBounds();
    }
}
