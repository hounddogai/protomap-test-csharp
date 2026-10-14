namespace Acme.Workforce.Compliance.Tests.Exception.Audit;

using Acme.Workforce.Compliance.Exception.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class RegionalExceptionAuditViewChecks
{
    public static void RegionalExceptionAuditViewShowsTheStatusInCapitals()
    {
        var item = RegionalExceptionAudit.NewRegionalExceptionAudit("code-972", "name-764");
        var view = RegionalExceptionAuditView.ToRegionalExceptionAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalExceptionAuditView.ToRegionalExceptionAuditViews(new List<RegionalExceptionAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalExceptionAuditViewShowsTheStatusInCapitals();
    }
}
