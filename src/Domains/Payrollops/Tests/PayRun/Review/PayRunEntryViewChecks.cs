namespace Acme.Workforce.Payrollops.Tests.PayRun.Review;

using Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayRunEntryViewChecks
{
    public static void PayRunEntryViewShowsTheStatusInCapitals()
    {
        var item = PayRunEntry.NewPayRunEntry("code-547", "title-683");
        var view = PayRunEntryView.ToPayRunEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayRunEntryView.ToPayRunEntryViews(new List<PayRunEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayRunEntryViewShowsTheStatusInCapitals();
    }
}
