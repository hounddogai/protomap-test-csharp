namespace Acme.Workforce.Talent.Tests.Mobility.Audit;

using Acme.Workforce.Talent.Mobility.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class MobilityAuditViewChecks
{
    public static void MobilityAuditViewShowsTheStatusInCapitals()
    {
        var item = MobilityAudit.NewMobilityAudit("code-253", "title-965");
        var view = MobilityAuditView.ToMobilityAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityAuditView.ToMobilityAuditViews(new List<MobilityAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityAuditViewShowsTheStatusInCapitals();
    }
}
