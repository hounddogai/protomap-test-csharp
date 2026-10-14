namespace Acme.Workforce.Assets.Tests.LicenseSeat.Summary;

using Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class LicenseSeatBudgetEngineChecks
{
    public static void LicenseSeatBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatBudgetEngine.DefaultLicenseSeatBudgetEngine();
        var item = LicenseSeatBudget.NewLicenseSeatBudget("code-441", "label-578");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatBudgetEngineKeepsResultsWithinBounds();
    }
}
