namespace Acme.Workforce.Absence.Tests.Accrual.Baseline;

using Acme.Workforce.Absence.Accrual.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AccrualBaselineViewChecks
{
    public static void AccrualBaselineViewShowsTheStatusInCapitals()
    {
        var item = AccrualBaseline.NewAccrualBaseline("code-100", "title-189");
        var view = AccrualBaselineView.ToAccrualBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualBaselineView.ToAccrualBaselineViews(new List<AccrualBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualBaselineViewShowsTheStatusInCapitals();
    }
}
