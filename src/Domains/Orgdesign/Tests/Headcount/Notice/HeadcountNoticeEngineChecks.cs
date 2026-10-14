namespace Acme.Workforce.Orgdesign.Tests.Headcount.Notice;

using Acme.Workforce.Orgdesign.Headcount.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class HeadcountNoticeEngineChecks
{
    public static void HeadcountNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountNoticeEngine.DefaultHeadcountNoticeEngine();
        var item = HeadcountNotice.NewHeadcountNotice("code-706", "label-358");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountNoticeEngineKeepsResultsWithinBounds();
    }
}
