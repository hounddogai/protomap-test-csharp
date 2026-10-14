namespace Acme.Workforce.Payrollops.Tests.Deduction.Baseline;

using Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LenientDeductionNoticeViewChecks
{
    public static void LenientDeductionNoticeViewShowsTheStatusInCapitals()
    {
        var item = LenientDeductionNotice.NewLenientDeductionNotice("code-779", "label-697");
        var view = LenientDeductionNoticeView.ToLenientDeductionNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientDeductionNoticeView.ToLenientDeductionNoticeViews(new List<LenientDeductionNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientDeductionNoticeViewShowsTheStatusInCapitals();
    }
}
