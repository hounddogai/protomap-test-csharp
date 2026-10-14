namespace Acme.Workforce.Compliance.Tests.Exception.Manifest;

using Acme.Workforce.Compliance.Exception.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ExceptionAuditViewChecks
{
    public static void ExceptionAuditViewShowsTheStatusInCapitals()
    {
        var item = ExceptionAudit.NewExceptionAudit("code-109", "label-264");
        var view = ExceptionAuditView.ToExceptionAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionAuditView.ToExceptionAuditViews(new List<ExceptionAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionAuditViewShowsTheStatusInCapitals();
    }
}
