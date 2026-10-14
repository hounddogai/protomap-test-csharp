namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AdaptivePolicyScoreEngineChecks
{
    public static void AdaptivePolicyScoreEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptivePolicyScoreEngine.DefaultAdaptivePolicyScoreEngine(Flags.NewRegistry());
        var item = AdaptivePolicyScore.NewAdaptivePolicyScore("code-465", "title-780");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptivePolicyScoreEngineKeepsResultsWithinBounds();
    }
}
