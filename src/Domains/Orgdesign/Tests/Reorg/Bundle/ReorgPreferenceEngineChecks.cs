namespace Acme.Workforce.Orgdesign.Tests.Reorg.Bundle;

using Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ReorgPreferenceEngineChecks
{
    public static void ReorgPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgPreferenceEngine.DefaultReorgPreferenceEngine();
        var item = ReorgPreference.NewReorgPreference("code-783", "title-96");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgPreferenceEngineKeepsResultsWithinBounds();
    }
}
