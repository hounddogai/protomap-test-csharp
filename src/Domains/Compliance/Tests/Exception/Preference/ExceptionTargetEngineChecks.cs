namespace Acme.Workforce.Compliance.Tests.Exception.Preference;

using Acme.Workforce.Compliance.Exception.Preference;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ExceptionTargetEngineChecks
{
    public static void ExceptionTargetEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionTargetEngine.DefaultExceptionTargetEngine(Flags.NewRegistry());
        var item = ExceptionTarget.NewExceptionTarget("code-359", "label-952");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionTargetEngineKeepsResultsWithinBounds();
    }
}
