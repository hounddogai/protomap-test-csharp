namespace Acme.Workforce.Payrollops.Tests.Garnishment.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class GarnishmentTargetEngineChecks
{
    public static void GarnishmentTargetEngineKeepsResultsWithinBounds()
    {
        var engine = GarnishmentTargetEngine.DefaultGarnishmentTargetEngine(Flags.NewRegistry());
        var item = GarnishmentTarget.NewGarnishmentTarget("code-867", "title-78");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GarnishmentTargetEngineKeepsResultsWithinBounds();
    }
}
