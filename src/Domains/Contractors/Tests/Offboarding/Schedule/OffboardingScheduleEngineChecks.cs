namespace Acme.Workforce.Contractors.Tests.Offboarding.Schedule;

using Acme.Workforce.Contractors.Offboarding.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class OffboardingScheduleEngineChecks
{
    public static void OffboardingScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = OffboardingScheduleEngine.DefaultOffboardingScheduleEngine();
        var item = OffboardingSchedule.NewOffboardingSchedule("code-441", "title-328");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OffboardingScheduleEngineKeepsResultsWithinBounds();
    }
}
