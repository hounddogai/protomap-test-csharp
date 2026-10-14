namespace Acme.Workforce.Workplace.Tests.Desk.Entry;

using Acme.Workforce.Workplace.Desk.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DeskQuotaViewChecks
{
    public static void DeskQuotaViewShowsTheStatusInCapitals()
    {
        var item = DeskQuota.NewDeskQuota("code-555", "title-66");
        var view = DeskQuotaView.ToDeskQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskQuotaView.ToDeskQuotaViews(new List<DeskQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskQuotaViewShowsTheStatusInCapitals();
    }
}
