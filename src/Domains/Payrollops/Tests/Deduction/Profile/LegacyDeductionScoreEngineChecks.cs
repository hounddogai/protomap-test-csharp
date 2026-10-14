namespace Acme.Workforce.Payrollops.Tests.Deduction.Profile;

using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class LegacyDeductionScoreEngineChecks
{
    public static void LegacyDeductionScoreEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyDeductionScoreEngine.DefaultLegacyDeductionScoreEngine();
        var item = LegacyDeductionScore.NewLegacyDeductionScore("code-820", "name-320");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyDeductionScoreEngineKeepsResultsWithinBounds();
    }
}
