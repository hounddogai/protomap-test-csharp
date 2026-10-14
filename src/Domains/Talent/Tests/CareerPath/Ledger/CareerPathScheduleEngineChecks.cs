namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathScheduleEngineChecks
{
    public static void CareerPathScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = CareerPathScheduleEngine.DefaultCareerPathScheduleEngine();
        var item = CareerPathSchedule.NewCareerPathSchedule("code-56", "label-427");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CareerPathScheduleEngineKeepsResultsWithinBounds();
    }
}
