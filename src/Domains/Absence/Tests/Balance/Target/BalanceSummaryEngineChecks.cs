namespace Acme.Workforce.Absence.Tests.Balance.Target;

using Acme.Workforce.Absence.Balance.Target;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class BalanceSummaryEngineChecks
{
    public static void BalanceSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = BalanceSummaryEngine.DefaultBalanceSummaryEngine(Flags.NewRegistry());
        var item = BalanceSummary.NewBalanceSummary("code-165", "name-165");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BalanceSummaryEngineKeepsResultsWithinBounds();
    }
}
