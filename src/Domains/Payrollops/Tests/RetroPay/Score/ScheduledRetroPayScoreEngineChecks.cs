namespace Acme.Workforce.Payrollops.Tests.RetroPay.Score;

using Acme.Workforce.Payrollops.RetroPay.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ScheduledRetroPayScoreEngineChecks
{
    public static void ScheduledRetroPayScoreEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledRetroPayScoreEngine.DefaultScheduledRetroPayScoreEngine();
        var item = ScheduledRetroPayScore.NewScheduledRetroPayScore("code-686", "title-741");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledRetroPayScoreEngineKeepsResultsWithinBounds();
    }
}
