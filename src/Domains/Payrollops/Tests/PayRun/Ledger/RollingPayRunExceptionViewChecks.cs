namespace Acme.Workforce.Payrollops.Tests.PayRun.Ledger;

using Acme.Workforce.Payrollops.PayRun.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RollingPayRunExceptionViewChecks
{
    public static void RollingPayRunExceptionViewShowsTheStatusInCapitals()
    {
        var item = RollingPayRunException.NewRollingPayRunException("code-628", "name-722");
        var view = RollingPayRunExceptionView.ToRollingPayRunExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingPayRunExceptionView.ToRollingPayRunExceptionViews(new List<RollingPayRunException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingPayRunExceptionViewShowsTheStatusInCapitals();
    }
}
