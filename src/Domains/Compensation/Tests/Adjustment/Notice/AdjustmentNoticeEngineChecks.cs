namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentNoticeEngineChecks
{
    public static void AdjustmentNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentNoticeEngine.DefaultAdjustmentNoticeEngine();
        var item = AdjustmentNotice.NewAdjustmentNotice("code-393", "label-19");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentNoticeEngineKeepsResultsWithinBounds();
    }
}
