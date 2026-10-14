namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingSlot;

using Acme.Workforce.Contractors.Offboarding.OffboardingSlot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of offboarding slot code, run by the project's runner.</summary>
public static class LegacyOffboardingForecastEngineChecks
{
    public static void LegacyOffboardingForecastEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyOffboardingForecastEngine.DefaultLegacyOffboardingForecastEngine(Flags.NewRegistry());
        var item = LegacyOffboardingForecast.NewLegacyOffboardingForecast("code-440", "label-686");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyOffboardingForecastEngineKeepsResultsWithinBounds();
    }
}
