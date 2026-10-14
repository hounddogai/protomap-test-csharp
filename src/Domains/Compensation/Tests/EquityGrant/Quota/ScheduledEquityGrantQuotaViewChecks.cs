namespace Acme.Workforce.Compensation.Tests.EquityGrant.Quota;

using Acme.Workforce.Compensation.EquityGrant.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ScheduledEquityGrantQuotaViewChecks
{
    public static void ScheduledEquityGrantQuotaViewShowsTheStatusInCapitals()
    {
        var item = ScheduledEquityGrantQuota.NewScheduledEquityGrantQuota("code-60", "title-629");
        var view = ScheduledEquityGrantQuotaView.ToScheduledEquityGrantQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledEquityGrantQuotaView.ToScheduledEquityGrantQuotaViews(new List<ScheduledEquityGrantQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledEquityGrantQuotaViewShowsTheStatusInCapitals();
    }
}
