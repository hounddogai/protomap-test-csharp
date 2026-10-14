namespace Acme.Workforce.Compliance.Tests.Exception.Rule;

using Acme.Workforce.Compliance.Exception.Rule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FallbackExceptionAssignmentEngineChecks
{
    public static void FallbackExceptionAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackExceptionAssignmentEngine.DefaultFallbackExceptionAssignmentEngine(Flags.NewRegistry());
        var item = FallbackExceptionAssignment.NewFallbackExceptionAssignment("code-45", "name-352");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackExceptionAssignmentEngineKeepsResultsWithinBounds();
    }
}
