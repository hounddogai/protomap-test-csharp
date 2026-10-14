namespace Acme.Workforce.Compensation.Tests.EquityGrant.Group;

using Acme.Workforce.Compensation.EquityGrant.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class EquityGrantQuotaViewChecks
{
    public static void EquityGrantQuotaViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantQuota.NewEquityGrantQuota("code-672", "name-911");
        var view = EquityGrantQuotaView.ToEquityGrantQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantQuotaView.ToEquityGrantQuotaViews(new List<EquityGrantQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantQuotaViewShowsTheStatusInCapitals();
    }
}
