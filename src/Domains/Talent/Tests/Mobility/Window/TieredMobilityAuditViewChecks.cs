namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TieredMobilityAuditViewChecks
{
    public static void TieredMobilityAuditViewShowsTheStatusInCapitals()
    {
        var item = TieredMobilityAudit.NewTieredMobilityAudit("code-772", "name-766");
        var view = TieredMobilityAuditView.ToTieredMobilityAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredMobilityAuditView.ToTieredMobilityAuditViews(new List<TieredMobilityAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredMobilityAuditViewShowsTheStatusInCapitals();
    }
}
