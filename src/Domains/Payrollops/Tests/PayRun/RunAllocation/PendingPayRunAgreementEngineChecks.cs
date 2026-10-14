namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAllocation;

using Acme.Workforce.Payrollops.PayRun.RunAllocation;

/// <summary>Checks of run allocation code, run by the project's runner.</summary>
public static class PendingPayRunAgreementEngineChecks
{
    public static void PendingPayRunAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = PendingPayRunAgreementEngine.DefaultPendingPayRunAgreementEngine();
        var item = PendingPayRunAgreement.NewPendingPayRunAgreement("code-509", "title-140");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingPayRunAgreementEngineKeepsResultsWithinBounds();
    }
}
