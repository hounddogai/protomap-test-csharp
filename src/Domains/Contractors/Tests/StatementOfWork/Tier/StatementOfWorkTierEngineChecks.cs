namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Tier;

using Acme.Workforce.Contractors.StatementOfWork.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StatementOfWorkTierEngineChecks
{
    public static void StatementOfWorkTierEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkTierEngine.DefaultStatementOfWorkTierEngine();
        var item = StatementOfWorkTier.NewStatementOfWorkTier("code-871", "label-379");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkTierEngineKeepsResultsWithinBounds();
    }
}
