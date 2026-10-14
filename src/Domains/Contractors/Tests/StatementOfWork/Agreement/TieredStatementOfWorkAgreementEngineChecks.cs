namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Agreement;

using Acme.Workforce.Contractors.StatementOfWork.Agreement;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class TieredStatementOfWorkAgreementEngineChecks
{
    public static void TieredStatementOfWorkAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = TieredStatementOfWorkAgreementEngine.DefaultTieredStatementOfWorkAgreementEngine(Flags.NewRegistry());
        var item = TieredStatementOfWorkAgreement.NewTieredStatementOfWorkAgreement("code-770", "name-554");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredStatementOfWorkAgreementEngineKeepsResultsWithinBounds();
    }
}
