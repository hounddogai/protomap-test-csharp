namespace Acme.Workforce.Payrollops.Tests.Payslip.Checkpoint;

using Acme.Workforce.Payrollops.Payslip.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PayslipProposalEngineChecks
{
    public static void PayslipProposalEngineKeepsResultsWithinBounds()
    {
        var engine = PayslipProposalEngine.DefaultPayslipProposalEngine();
        var item = PayslipProposal.NewPayslipProposal("code-18", "title-345");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayslipProposalEngineKeepsResultsWithinBounds();
    }
}
