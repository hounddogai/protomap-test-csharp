namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LegacyStatementOfWorkAgreementEngineChecks
{
    public static void LegacyStatementOfWorkAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyStatementOfWorkAgreementEngine.DefaultLegacyStatementOfWorkAgreementEngine();
        var item = LegacyStatementOfWorkAgreement.NewLegacyStatementOfWorkAgreement("code-846", "title-86");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyStatementOfWorkAgreementEngineKeepsResultsWithinBounds();
    }
}
