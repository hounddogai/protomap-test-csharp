namespace Acme.Workforce.Assets.Tests.LicenseSeat.Baseline;

using Acme.Workforce.Assets.LicenseSeat.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PrimaryLicenseSeatWindowEngineChecks
{
    public static void PrimaryLicenseSeatWindowEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryLicenseSeatWindowEngine.DefaultPrimaryLicenseSeatWindowEngine(Flags.NewRegistry());
        var item = PrimaryLicenseSeatWindow.NewPrimaryLicenseSeatWindow("code-812", "label-136");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryLicenseSeatWindowEngineKeepsResultsWithinBounds();
    }
}
