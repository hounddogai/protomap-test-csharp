namespace Acme.Workforce.Contractors.Tests.Offboarding.Cycle;

using Acme.Workforce.Contractors.Offboarding.Cycle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class OffboardingCycleEngineChecks
{
    public static void OffboardingCycleEngineKeepsResultsWithinBounds()
    {
        var engine = OffboardingCycleEngine.DefaultOffboardingCycleEngine(Flags.NewRegistry());
        var item = OffboardingCycle.NewOffboardingCycle("code-776", "name-431");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OffboardingCycleEngineKeepsResultsWithinBounds();
    }
}
