namespace Acme.Workforce.Compliance.Tests.Audit.Mapping;

using Acme.Workforce.Compliance.Audit.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AuditTemplateViewChecks
{
    public static void AuditTemplateViewShowsTheStatusInCapitals()
    {
        var item = AuditTemplate.NewAuditTemplate("code-611", "title-116");
        var view = AuditTemplateView.ToAuditTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AuditTemplateView.ToAuditTemplateViews(new List<AuditTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AuditTemplateViewShowsTheStatusInCapitals();
    }
}
