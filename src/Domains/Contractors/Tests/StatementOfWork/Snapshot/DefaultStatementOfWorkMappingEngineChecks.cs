namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class DefaultStatementOfWorkMappingEngineChecks
{
    public static void DefaultStatementOfWorkMappingEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultStatementOfWorkMappingEngine.DefaultDefaultStatementOfWorkMappingEngine(Flags.NewRegistry());
        var item = DefaultStatementOfWorkMapping.NewDefaultStatementOfWorkMapping("code-733", "label-326");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultStatementOfWorkMappingEngineKeepsResultsWithinBounds();
    }
}
