namespace Acme.Workforce.Contractors.Tests.Offboarding.Bundle;

using Acme.Workforce.Contractors.Offboarding.Bundle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class OffboardingAssignmentEngineChecks
{
    public static void OffboardingAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = OffboardingAssignmentEngine.DefaultOffboardingAssignmentEngine(Flags.NewRegistry());
        var item = OffboardingAssignment.NewOffboardingAssignment("code-897", "label-188");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OffboardingAssignmentEngineKeepsResultsWithinBounds();
    }
}
