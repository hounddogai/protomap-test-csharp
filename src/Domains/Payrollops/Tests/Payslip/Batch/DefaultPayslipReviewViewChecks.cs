namespace Acme.Workforce.Payrollops.Tests.Payslip.Batch;

using Acme.Workforce.Payrollops.Payslip.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DefaultPayslipReviewViewChecks
{
    public static void DefaultPayslipReviewViewShowsTheStatusInCapitals()
    {
        var item = DefaultPayslipReview.NewDefaultPayslipReview("code-641", "title-543");
        var view = DefaultPayslipReviewView.ToDefaultPayslipReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultPayslipReviewView.ToDefaultPayslipReviewViews(new List<DefaultPayslipReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultPayslipReviewViewShowsTheStatusInCapitals();
    }
}
