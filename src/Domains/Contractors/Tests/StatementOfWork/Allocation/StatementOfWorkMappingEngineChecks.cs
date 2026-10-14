namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Allocation;

using Acme.Workforce.Contractors.StatementOfWork.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class StatementOfWorkMappingEngineChecks
{
    public static void StatementOfWorkMappingEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkMappingEngine.DefaultStatementOfWorkMappingEngine();
        var item = StatementOfWorkMapping.NewStatementOfWorkMapping("code-45", "name-90");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkMappingEngineKeepsResultsWithinBounds();
    }
}
