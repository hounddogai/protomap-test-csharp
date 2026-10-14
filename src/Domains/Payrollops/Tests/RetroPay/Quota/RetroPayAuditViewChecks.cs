namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RetroPayAuditViewChecks
{
    public static void RetroPayAuditViewShowsTheStatusInCapitals()
    {
        var item = RetroPayAudit.NewRetroPayAudit("code-500", "name-39");
        var view = RetroPayAuditView.ToRetroPayAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPayAuditView.ToRetroPayAuditViews(new List<RetroPayAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPayAuditViewShowsTheStatusInCapitals();
    }
}
