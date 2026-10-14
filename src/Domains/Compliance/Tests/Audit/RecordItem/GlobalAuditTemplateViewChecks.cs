namespace Acme.Workforce.Compliance.Tests.Audit.RecordItem;

using Acme.Workforce.Compliance.Audit.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class GlobalAuditTemplateViewChecks
{
    public static void GlobalAuditTemplateViewShowsTheStatusInCapitals()
    {
        var item = GlobalAuditTemplate.NewGlobalAuditTemplate("code-513", "label-766");
        var view = GlobalAuditTemplateView.ToGlobalAuditTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalAuditTemplateView.ToGlobalAuditTemplateViews(new List<GlobalAuditTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalAuditTemplateViewShowsTheStatusInCapitals();
    }
}
