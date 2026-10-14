namespace Acme.Workforce.Assets.Tests.LicenseSeat.Audit;

using Acme.Workforce.Assets.LicenseSeat.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LicenseSeatProfileEngineChecks
{
    public static void LicenseSeatProfileEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatProfileEngine.DefaultLicenseSeatProfileEngine();
        var item = LicenseSeatProfile.NewLicenseSeatProfile("code-979", "title-747");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatProfileEngineKeepsResultsWithinBounds();
    }
}
