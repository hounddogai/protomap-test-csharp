namespace Acme.Workforce.Payrollops.Tests.PayRun.Bundle;

using Acme.Workforce.Payrollops.PayRun.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PayRunExceptionViewChecks
{
    public static void PayRunExceptionViewShowsTheStatusInCapitals()
    {
        var item = PayRunException.NewPayRunException("code-314", "label-372");
        var view = PayRunExceptionView.ToPayRunExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayRunExceptionView.ToPayRunExceptionViews(new List<PayRunException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayRunExceptionViewShowsTheStatusInCapitals();
    }
}
