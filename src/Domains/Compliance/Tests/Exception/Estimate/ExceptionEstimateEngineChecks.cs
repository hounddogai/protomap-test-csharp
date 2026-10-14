namespace Acme.Workforce.Compliance.Tests.Exception.Estimate;

using Acme.Workforce.Compliance.Exception.Estimate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ExceptionEstimateEngineChecks
{
    public static void ExceptionEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionEstimateEngine.DefaultExceptionEstimateEngine(Flags.NewRegistry());
        var item = ExceptionEstimate.NewExceptionEstimate("code-196", "name-877");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionEstimateEngineKeepsResultsWithinBounds();
    }
}
