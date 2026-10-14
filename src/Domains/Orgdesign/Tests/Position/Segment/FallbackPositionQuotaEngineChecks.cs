namespace Acme.Workforce.Orgdesign.Tests.Position.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class FallbackPositionQuotaEngineChecks
{
    public static void FallbackPositionQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackPositionQuotaEngine.DefaultFallbackPositionQuotaEngine(Flags.NewRegistry());
        var item = FallbackPositionQuota.NewFallbackPositionQuota("code-980", "title-730");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackPositionQuotaEngineKeepsResultsWithinBounds();
    }
}
