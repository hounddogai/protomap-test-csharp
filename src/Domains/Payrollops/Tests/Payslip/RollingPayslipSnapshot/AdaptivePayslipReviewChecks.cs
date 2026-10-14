namespace Acme.Workforce.Payrollops.Tests.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

/// <summary>Checks of rolling payslip snapshot code, run by the project's runner.</summary>
public static class AdaptivePayslipReviewChecks
{
    public static void AdaptivePayslipReviewStartsInRequested()
    {
        var item = AdaptivePayslipReview.NewAdaptivePayslipReview("code-751", "name-822");
        Check.Equal(item.Status, AdaptivePayslipReviewStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptivePayslipReviewMovesToRevoked()
    {
        var item = AdaptivePayslipReview.NewAdaptivePayslipReview("code-751", "name-822");
        try
        {
            item.MoveTo(AdaptivePayslipReviewStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptivePayslipReviewStatus.Revoked, "status");
    }

    public static void AdaptivePayslipReviewRejectsMovesFromExpired()
    {
        var item = AdaptivePayslipReview.NewAdaptivePayslipReview("code-751", "name-822");
        item.Status = AdaptivePayslipReviewStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptivePayslipReviewStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptivePayslipReviewStartsInRequested();
        AdaptivePayslipReviewMovesToRevoked();
        AdaptivePayslipReviewRejectsMovesFromExpired();
    }
}
