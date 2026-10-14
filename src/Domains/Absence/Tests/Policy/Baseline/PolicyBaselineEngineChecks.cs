namespace Acme.Workforce.Absence.Tests.Policy.Baseline;

using Acme.Workforce.Absence.Policy.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PolicyBaselineEngineChecks
{
    public static void PolicyBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyBaselineEngine.DefaultPolicyBaselineEngine();
        var item = PolicyBaseline.NewPolicyBaseline("code-772", "name-559");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyBaselineEngineKeepsResultsWithinBounds();
    }
}
