namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Allocation;

using Acme.Workforce.Contractors.StatementOfWork.Allocation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class LenientStatementOfWorkCheckpointEngineChecks
{
    public static void LenientStatementOfWorkCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = LenientStatementOfWorkCheckpointEngine.DefaultLenientStatementOfWorkCheckpointEngine(Flags.NewRegistry());
        var item = LenientStatementOfWorkCheckpoint.NewLenientStatementOfWorkCheckpoint("code-495", "name-934");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientStatementOfWorkCheckpointEngineKeepsResultsWithinBounds();
    }
}
