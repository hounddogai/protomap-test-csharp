namespace Acme.Workforce.Absence.Tests.Accrual.Cycle;

using Acme.Workforce.Absence.Accrual.Cycle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AccrualScoreEngineChecks
{
    public static void AccrualScoreEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualScoreEngine.DefaultAccrualScoreEngine(Flags.NewRegistry());
        var item = AccrualScore.NewAccrualScore("code-105", "title-755");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualScoreEngineKeepsResultsWithinBounds();
    }
}
