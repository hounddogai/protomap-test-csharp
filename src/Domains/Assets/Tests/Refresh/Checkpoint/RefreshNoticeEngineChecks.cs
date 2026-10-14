namespace Acme.Workforce.Assets.Tests.Refresh.Checkpoint;

using Acme.Workforce.Assets.Refresh.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RefreshNoticeEngineChecks
{
    public static void RefreshNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshNoticeEngine.DefaultRefreshNoticeEngine();
        var item = RefreshNotice.NewRefreshNotice("code-371", "name-26");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshNoticeEngineKeepsResultsWithinBounds();
    }
}
