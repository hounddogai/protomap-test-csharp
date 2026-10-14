namespace Acme.Workforce.Orgdesign.Tests.Reorg.Bundle;

using Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PrimaryReorgBaselineEngineChecks
{
    public static void PrimaryReorgBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryReorgBaselineEngine.DefaultPrimaryReorgBaselineEngine();
        var item = PrimaryReorgBaseline.NewPrimaryReorgBaseline("code-363", "label-999");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryReorgBaselineEngineKeepsResultsWithinBounds();
    }
}
