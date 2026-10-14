namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StatementOfWorkDigestEngineChecks
{
    public static void StatementOfWorkDigestEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkDigestEngine.DefaultStatementOfWorkDigestEngine();
        var item = StatementOfWorkDigest.NewStatementOfWorkDigest("code-891", "name-450");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkDigestEngineKeepsResultsWithinBounds();
    }
}
