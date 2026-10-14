namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SeasonalLicenseSeatPreferenceEngineChecks
{
    public static void SeasonalLicenseSeatPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalLicenseSeatPreferenceEngine.DefaultSeasonalLicenseSeatPreferenceEngine(Flags.NewRegistry());
        var item = SeasonalLicenseSeatPreference.NewSeasonalLicenseSeatPreference("code-435", "name-136");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalLicenseSeatPreferenceEngineKeepsResultsWithinBounds();
    }
}
