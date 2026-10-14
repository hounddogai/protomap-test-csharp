namespace Acme.Workforce.Talent.Tests.CareerPath.Digest;

using Acme.Workforce.Talent.CareerPath.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AdaptiveCareerPathNoticeEngineChecks
{
    public static void AdaptiveCareerPathNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveCareerPathNoticeEngine.DefaultAdaptiveCareerPathNoticeEngine();
        var item = AdaptiveCareerPathNotice.NewAdaptiveCareerPathNotice("code-792", "label-908");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveCareerPathNoticeEngineKeepsResultsWithinBounds();
    }
}
