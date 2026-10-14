namespace Acme.Workforce.Absence.Tests.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.Variance;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AdaptiveHolidayVarianceEngineChecks
{
    public static void AdaptiveHolidayVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveHolidayVarianceEngine.DefaultAdaptiveHolidayVarianceEngine(Flags.NewRegistry());
        var item = AdaptiveHolidayVariance.NewAdaptiveHolidayVariance("code-800", "label-489");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveHolidayVarianceEngineKeepsResultsWithinBounds();
    }
}
