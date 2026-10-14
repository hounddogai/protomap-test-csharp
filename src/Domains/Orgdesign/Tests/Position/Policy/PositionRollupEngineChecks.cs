namespace Acme.Workforce.Orgdesign.Tests.Position.Policy;

using Acme.Workforce.Orgdesign.Position.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PositionRollupEngineChecks
{
    public static void PositionRollupEngineKeepsResultsWithinBounds()
    {
        var engine = PositionRollupEngine.DefaultPositionRollupEngine();
        var item = PositionRollup.NewPositionRollup("code-885", "title-89");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PositionRollupEngineKeepsResultsWithinBounds();
    }
}
