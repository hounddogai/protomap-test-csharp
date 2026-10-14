namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SeasonalLicenseSeatWindowEngineChecks
{
    public static void SeasonalLicenseSeatWindowEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalLicenseSeatWindowEngine.DefaultSeasonalLicenseSeatWindowEngine(Flags.NewRegistry());
        var item = SeasonalLicenseSeatWindow.NewSeasonalLicenseSeatWindow("code-626", "title-437");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalLicenseSeatWindowEngineKeepsResultsWithinBounds();
    }
}
