namespace Acme.Workforce.Talent.Tests.CareerPath.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ScheduledCareerPathProfileEngineChecks
{
    public static void ScheduledCareerPathProfileEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledCareerPathProfileEngine.DefaultScheduledCareerPathProfileEngine(Flags.NewRegistry());
        var item = ScheduledCareerPathProfile.NewScheduledCareerPathProfile("code-38", "title-752");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledCareerPathProfileEngineKeepsResultsWithinBounds();
    }
}
