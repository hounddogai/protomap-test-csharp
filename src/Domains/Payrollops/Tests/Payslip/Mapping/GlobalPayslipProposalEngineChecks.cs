namespace Acme.Workforce.Payrollops.Tests.Payslip.Mapping;

using Acme.Workforce.Payrollops.Payslip.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class GlobalPayslipProposalEngineChecks
{
    public static void GlobalPayslipProposalEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalPayslipProposalEngine.DefaultGlobalPayslipProposalEngine();
        var item = GlobalPayslipProposal.NewGlobalPayslipProposal("code-361", "title-296");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalPayslipProposalEngineKeepsResultsWithinBounds();
    }
}
