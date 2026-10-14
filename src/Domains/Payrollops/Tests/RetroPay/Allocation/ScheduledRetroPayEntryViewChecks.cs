namespace Acme.Workforce.Payrollops.Tests.RetroPay.Allocation;

using Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ScheduledRetroPayEntryViewChecks
{
    public static void ScheduledRetroPayEntryViewShowsTheStatusInCapitals()
    {
        var item = ScheduledRetroPayEntry.NewScheduledRetroPayEntry("code-134", "title-273");
        var view = ScheduledRetroPayEntryView.ToScheduledRetroPayEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledRetroPayEntryView.ToScheduledRetroPayEntryViews(new List<ScheduledRetroPayEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledRetroPayEntryViewShowsTheStatusInCapitals();
    }
}
