namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayslipSummaryChecks
{
    public static void PayslipSummaryStartsInRequested()
    {
        var item = PayslipSummary.NewPayslipSummary("code-585", "name-840");
        Check.Equal(item.Status, PayslipSummaryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipSummaryMovesToRevoked()
    {
        var item = PayslipSummary.NewPayslipSummary("code-585", "name-840");
        try
        {
            item.MoveTo(PayslipSummaryStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipSummaryStatus.Revoked, "status");
    }

    public static void PayslipSummaryRejectsMovesFromExpired()
    {
        var item = PayslipSummary.NewPayslipSummary("code-585", "name-840");
        item.Status = PayslipSummaryStatus.Expired;
        Check.Fails(() => item.MoveTo(PayslipSummaryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PayslipSummaryStartsInRequested();
        PayslipSummaryMovesToRevoked();
        PayslipSummaryRejectsMovesFromExpired();
    }
}
