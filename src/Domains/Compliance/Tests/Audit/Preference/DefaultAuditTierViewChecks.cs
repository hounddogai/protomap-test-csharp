namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DefaultAuditTierViewChecks
{
    public static void DefaultAuditTierViewShowsTheStatusInCapitals()
    {
        var item = DefaultAuditTier.NewDefaultAuditTier("code-926", "label-709");
        var view = DefaultAuditTierView.ToDefaultAuditTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultAuditTierView.ToDefaultAuditTierViews(new List<DefaultAuditTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultAuditTierViewShowsTheStatusInCapitals();
    }
}
