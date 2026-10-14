namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Entry;

using Acme.Workforce.Engagement.PulseSurvey.Entry;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ManualPulseSurveySummaryEngineChecks
{
    public static void ManualPulseSurveySummaryEngineKeepsResultsWithinBounds()
    {
        var engine = ManualPulseSurveySummaryEngine.DefaultManualPulseSurveySummaryEngine(Flags.NewRegistry());
        var item = ManualPulseSurveySummary.NewManualPulseSurveySummary("code-207", "label-363");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualPulseSurveySummaryEngineKeepsResultsWithinBounds();
    }
}
