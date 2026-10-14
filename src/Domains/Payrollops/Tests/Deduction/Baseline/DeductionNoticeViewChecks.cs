namespace Acme.Workforce.Payrollops.Tests.Deduction.Baseline;

using Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DeductionNoticeViewChecks
{
    public static void DeductionNoticeViewShowsTheStatusInCapitals()
    {
        var item = DeductionNotice.NewDeductionNotice("code-369", "label-395");
        var view = DeductionNoticeView.ToDeductionNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeductionNoticeView.ToDeductionNoticeViews(new List<DeductionNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeductionNoticeViewShowsTheStatusInCapitals();
    }
}
