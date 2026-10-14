namespace Acme.Workforce.Assets.Tests.Assignment.Baseline;

using Acme.Workforce.Assets.Assignment.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AssignmentLedgerEngineChecks
{
    public static void AssignmentLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentLedgerEngine.DefaultAssignmentLedgerEngine(Flags.NewRegistry());
        var item = AssignmentLedger.NewAssignmentLedger("code-998", "label-957");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentLedgerEngineKeepsResultsWithinBounds();
    }
}
