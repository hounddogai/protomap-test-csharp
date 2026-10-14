namespace Acme.Workforce.Orgdesign.Tests.Position.PositionAudit;

using Acme.Workforce.Orgdesign.Position.PositionAudit;

/// <summary>Checks of position audit code, run by the project's runner.</summary>
public static class LegacyPositionAuditViewChecks
{
    public static void LegacyPositionAuditViewShowsTheStatusInCapitals()
    {
        var item = LegacyPositionAudit.NewLegacyPositionAudit("code-402", "title-197");
        var view = LegacyPositionAuditView.ToLegacyPositionAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyPositionAuditView.ToLegacyPositionAuditViews(new List<LegacyPositionAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyPositionAuditViewShowsTheStatusInCapitals();
    }
}
