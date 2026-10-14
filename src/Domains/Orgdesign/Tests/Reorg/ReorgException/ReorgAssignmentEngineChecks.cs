namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class ReorgAssignmentEngineChecks
{
    public static void ReorgAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgAssignmentEngine.DefaultReorgAssignmentEngine(Flags.NewRegistry());
        var item = ReorgAssignment.NewReorgAssignment("code-814", "title-183");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgAssignmentEngineKeepsResultsWithinBounds();
    }
}
