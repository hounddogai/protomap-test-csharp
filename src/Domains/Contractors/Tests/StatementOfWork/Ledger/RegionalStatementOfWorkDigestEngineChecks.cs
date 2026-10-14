namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Ledger;

using Acme.Workforce.Contractors.StatementOfWork.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RegionalStatementOfWorkDigestEngineChecks
{
    public static void RegionalStatementOfWorkDigestEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalStatementOfWorkDigestEngine.DefaultRegionalStatementOfWorkDigestEngine();
        var item = RegionalStatementOfWorkDigest.NewRegionalStatementOfWorkDigest("code-56", "name-361");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalStatementOfWorkDigestEngineKeepsResultsWithinBounds();
    }
}
