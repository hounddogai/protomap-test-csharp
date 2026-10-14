namespace Acme.Workforce.Assets.Tests.LicenseSeat.Summary;

using Acme.Workforce.Assets.LicenseSeat.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class LicenseSeatPreferenceEngineChecks
{
    public static void LicenseSeatPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatPreferenceEngine.DefaultLicenseSeatPreferenceEngine(Flags.NewRegistry());
        var item = LicenseSeatPreference.NewLicenseSeatPreference("code-938", "title-949");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatPreferenceEngineKeepsResultsWithinBounds();
    }
}
