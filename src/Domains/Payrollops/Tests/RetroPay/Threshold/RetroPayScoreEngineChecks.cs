namespace Acme.Workforce.Payrollops.Tests.RetroPay.Threshold;

using Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RetroPayScoreEngineChecks
{
    public static void RetroPayScoreEngineKeepsResultsWithinBounds()
    {
        var engine = RetroPayScoreEngine.DefaultRetroPayScoreEngine();
        var item = RetroPayScore.NewRetroPayScore("code-159", "label-501");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RetroPayScoreEngineKeepsResultsWithinBounds();
    }
}
