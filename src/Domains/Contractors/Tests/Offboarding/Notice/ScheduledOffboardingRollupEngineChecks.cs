namespace Acme.Workforce.Contractors.Tests.Offboarding.Notice;

using Acme.Workforce.Contractors.Offboarding.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScheduledOffboardingRollupEngineChecks
{
    public static void ScheduledOffboardingRollupEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledOffboardingRollupEngine.DefaultScheduledOffboardingRollupEngine(Flags.NewRegistry());
        var item = ScheduledOffboardingRollup.NewScheduledOffboardingRollup("code-999", "title-963");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledOffboardingRollupEngineKeepsResultsWithinBounds();
    }
}
