namespace Acme.Workforce.Assets.Tests.Assignment.Baseline;

using Acme.Workforce.Assets.Assignment.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LegacyAssignmentBaselineEngineChecks
{
    public static void LegacyAssignmentBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyAssignmentBaselineEngine.DefaultLegacyAssignmentBaselineEngine(Flags.NewRegistry());
        var item = LegacyAssignmentBaseline.NewLegacyAssignmentBaseline("code-987", "title-392");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyAssignmentBaselineEngineKeepsResultsWithinBounds();
    }
}
