namespace Acme.Workforce.Compliance.Tests.Audit.Mapping;

using Acme.Workforce.Compliance.Audit.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AuditMappingViewChecks
{
    public static void AuditMappingViewShowsTheStatusInCapitals()
    {
        var item = AuditMapping.NewAuditMapping("code-918", "name-9");
        var view = AuditMappingView.ToAuditMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AuditMappingView.ToAuditMappingViews(new List<AuditMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AuditMappingViewShowsTheStatusInCapitals();
    }
}
