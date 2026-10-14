namespace Acme.Workforce.Orgdesign.Tests.Position.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AdaptivePositionProfileEngineChecks
{
    public static void AdaptivePositionProfileEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptivePositionProfileEngine.DefaultAdaptivePositionProfileEngine(Flags.NewRegistry());
        var item = AdaptivePositionProfile.NewAdaptivePositionProfile("code-320", "title-450");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptivePositionProfileEngineKeepsResultsWithinBounds();
    }
}
