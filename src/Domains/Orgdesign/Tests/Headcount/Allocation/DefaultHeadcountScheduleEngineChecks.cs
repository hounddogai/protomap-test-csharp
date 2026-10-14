namespace Acme.Workforce.Orgdesign.Tests.Headcount.Allocation;

using Acme.Workforce.Orgdesign.Headcount.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class DefaultHeadcountScheduleEngineChecks
{
    public static void DefaultHeadcountScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultHeadcountScheduleEngine.DefaultDefaultHeadcountScheduleEngine();
        var item = DefaultHeadcountSchedule.NewDefaultHeadcountSchedule("code-81", "title-324");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultHeadcountScheduleEngineKeepsResultsWithinBounds();
    }
}
