namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Baseline;

using Acme.Workforce.Contractors.StatementOfWork.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PrimaryStatementOfWorkAllocationEngineChecks
{
    public static void PrimaryStatementOfWorkAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryStatementOfWorkAllocationEngine.DefaultPrimaryStatementOfWorkAllocationEngine(Flags.NewRegistry());
        var item = PrimaryStatementOfWorkAllocation.NewPrimaryStatementOfWorkAllocation("code-52", "label-407");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryStatementOfWorkAllocationEngineKeepsResultsWithinBounds();
    }
}
