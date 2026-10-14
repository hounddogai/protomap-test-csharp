namespace Acme.Workforce.Absence.Tests.Accrual.Threshold;

using Acme.Workforce.Absence.Accrual.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccrualSummaryViewChecks
{
    public static void AccrualSummaryViewShowsTheStatusInCapitals()
    {
        var item = AccrualSummary.NewAccrualSummary("code-816", "name-874");
        var view = AccrualSummaryView.ToAccrualSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualSummaryView.ToAccrualSummaryViews(new List<AccrualSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualSummaryViewShowsTheStatusInCapitals();
    }
}
