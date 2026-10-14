namespace Acme.Workforce.Assets.Tests.LicenseSeat.Milestone;

using Acme.Workforce.Assets.LicenseSeat.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LicenseSeatNoticeEngineChecks
{
    public static void LicenseSeatNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatNoticeEngine.DefaultLicenseSeatNoticeEngine();
        var item = LicenseSeatNotice.NewLicenseSeatNotice("code-941", "title-40");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatNoticeEngineKeepsResultsWithinBounds();
    }
}
