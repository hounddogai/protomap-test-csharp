namespace Acme.Workforce.Contractors.Tests.Rate.Batch;

using Acme.Workforce.Contractors.Rate.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RateNoticeEngineChecks
{
    public static void RateNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = RateNoticeEngine.DefaultRateNoticeEngine(Flags.NewRegistry());
        var item = RateNotice.NewRateNotice("code-383", "label-716");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RateNoticeEngineKeepsResultsWithinBounds();
    }
}
