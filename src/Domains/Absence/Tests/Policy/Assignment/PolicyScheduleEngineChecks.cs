namespace Acme.Workforce.Absence.Tests.Policy.Assignment;

using Acme.Workforce.Absence.Policy.Assignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PolicyScheduleEngineChecks
{
    public static void PolicyScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyScheduleEngine.DefaultPolicyScheduleEngine(Flags.NewRegistry());
        var item = PolicySchedule.NewPolicySchedule("code-932", "title-618");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyScheduleEngineKeepsResultsWithinBounds();
    }
}
