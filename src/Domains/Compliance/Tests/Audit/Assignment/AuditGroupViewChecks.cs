namespace Acme.Workforce.Compliance.Tests.Audit.Assignment;

using Acme.Workforce.Compliance.Audit.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AuditGroupViewChecks
{
    public static void AuditGroupViewShowsTheStatusInCapitals()
    {
        var item = AuditGroup.NewAuditGroup("code-325", "label-974");
        var view = AuditGroupView.ToAuditGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AuditGroupView.ToAuditGroupViews(new List<AuditGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AuditGroupViewShowsTheStatusInCapitals();
    }
}
