namespace Acme.Workforce.Compliance.Tests.Audit.AuditMapping;

using Acme.Workforce.Compliance.Audit.AuditMapping;

/// <summary>Checks of audit mapping code, run by the project's runner.</summary>
public static class DefaultAuditMappingViewChecks
{
    public static void DefaultAuditMappingViewShowsTheStatusInCapitals()
    {
        var item = DefaultAuditMapping.NewDefaultAuditMapping("code-392", "name-813");
        var view = DefaultAuditMappingView.ToDefaultAuditMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultAuditMappingView.ToDefaultAuditMappingViews(new List<DefaultAuditMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultAuditMappingViewShowsTheStatusInCapitals();
    }
}
