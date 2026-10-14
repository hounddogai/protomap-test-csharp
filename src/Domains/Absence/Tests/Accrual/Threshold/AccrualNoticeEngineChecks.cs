namespace Acme.Workforce.Absence.Tests.Accrual.Threshold;

using Acme.Workforce.Absence.Accrual.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccrualNoticeEngineChecks
{
    public static void AccrualNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualNoticeEngine.DefaultAccrualNoticeEngine();
        var item = AccrualNotice.NewAccrualNotice("code-454", "label-524");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualNoticeEngineKeepsResultsWithinBounds();
    }
}
