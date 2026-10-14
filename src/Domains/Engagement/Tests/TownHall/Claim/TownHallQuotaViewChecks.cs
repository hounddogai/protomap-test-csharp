namespace Acme.Workforce.Engagement.Tests.TownHall.Claim;

using Acme.Workforce.Engagement.TownHall.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class TownHallQuotaViewChecks
{
    public static void TownHallQuotaViewShowsTheStatusInCapitals()
    {
        var item = TownHallQuota.NewTownHallQuota("code-628", "title-753");
        var view = TownHallQuotaView.ToTownHallQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallQuotaView.ToTownHallQuotaViews(new List<TownHallQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallQuotaViewShowsTheStatusInCapitals();
    }
}
