namespace Acme.Workforce.Contractors.Tests.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

/// <summary>Checks of capped offboarding slot code, run by the project's runner.</summary>
public static class CappedOffboardingScheduleEngineChecks
{
    public static void CappedOffboardingScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = CappedOffboardingScheduleEngine.DefaultCappedOffboardingScheduleEngine();
        var item = CappedOffboardingSchedule.NewCappedOffboardingSchedule("code-80", "label-632");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedOffboardingScheduleEngineKeepsResultsWithinBounds();
    }
}
