namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentAllocation;

using Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

/// <summary>Checks of adjustment allocation code, run by the project's runner.</summary>
public static class RollingAdjustmentNoticeEngineChecks
{
    public static void RollingAdjustmentNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = RollingAdjustmentNoticeEngine.DefaultRollingAdjustmentNoticeEngine();
        var item = RollingAdjustmentNotice.NewRollingAdjustmentNotice("code-780", "title-672");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingAdjustmentNoticeEngineKeepsResultsWithinBounds();
    }
}
