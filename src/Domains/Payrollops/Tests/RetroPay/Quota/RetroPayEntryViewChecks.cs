namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RetroPayEntryViewChecks
{
    public static void RetroPayEntryViewShowsTheStatusInCapitals()
    {
        var item = RetroPayEntry.NewRetroPayEntry("code-729", "name-386");
        var view = RetroPayEntryView.ToRetroPayEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPayEntryView.ToRetroPayEntryViews(new List<RetroPayEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPayEntryViewShowsTheStatusInCapitals();
    }
}
