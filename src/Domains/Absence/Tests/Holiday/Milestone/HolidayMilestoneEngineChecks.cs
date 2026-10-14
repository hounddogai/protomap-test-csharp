namespace Acme.Workforce.Absence.Tests.Holiday.Milestone;

using Acme.Workforce.Absence.Holiday.Milestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class HolidayMilestoneEngineChecks
{
    public static void HolidayMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayMilestoneEngine.DefaultHolidayMilestoneEngine(Flags.NewRegistry());
        var item = HolidayMilestone.NewHolidayMilestone("code-918", "label-971");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayMilestoneEngineKeepsResultsWithinBounds();
    }
}
