namespace Acme.Workforce.Workplace.Tests.Visitor.Cycle;

using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class VisitorQuotaViewChecks
{
    public static void VisitorQuotaViewShowsTheStatusInCapitals()
    {
        var item = VisitorQuota.NewVisitorQuota("code-494", "label-594");
        var view = VisitorQuotaView.ToVisitorQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorQuotaView.ToVisitorQuotaViews(new List<VisitorQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorQuotaViewShowsTheStatusInCapitals();
    }
}
