namespace Acme.Workforce.Payrollops.Tests.PayRun.Audit;

using Acme.Workforce.Payrollops.PayRun.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PayRunAgreementEngineChecks
{
    public static void PayRunAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = PayRunAgreementEngine.DefaultPayRunAgreementEngine();
        var item = PayRunAgreement.NewPayRunAgreement("code-816", "name-763");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayRunAgreementEngineKeepsResultsWithinBounds();
    }
}
