namespace Acme.Workforce.Absence.Tests.Accrual.Threshold;

using Acme.Workforce.Absence.Accrual.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccrualNoticeViewChecks
{
    public static void AccrualNoticeViewShowsTheStatusInCapitals()
    {
        var item = AccrualNotice.NewAccrualNotice("code-454", "label-524");
        var view = AccrualNoticeView.ToAccrualNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualNoticeView.ToAccrualNoticeViews(new List<AccrualNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualNoticeViewShowsTheStatusInCapitals();
    }
}
