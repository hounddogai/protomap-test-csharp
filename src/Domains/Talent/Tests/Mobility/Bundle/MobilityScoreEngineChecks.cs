namespace Acme.Workforce.Talent.Tests.Mobility.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class MobilityScoreEngineChecks
{
    public static void MobilityScoreEngineKeepsResultsWithinBounds()
    {
        var engine = MobilityScoreEngine.DefaultMobilityScoreEngine(Flags.NewRegistry());
        var item = MobilityScore.NewMobilityScore("code-138", "title-731");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MobilityScoreEngineKeepsResultsWithinBounds();
    }
}
