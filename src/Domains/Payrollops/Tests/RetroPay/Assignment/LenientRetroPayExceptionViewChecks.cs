namespace Acme.Workforce.Payrollops.Tests.RetroPay.Assignment;

using Acme.Workforce.Payrollops.RetroPay.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LenientRetroPayExceptionViewChecks
{
    public static void LenientRetroPayExceptionViewShowsTheStatusInCapitals()
    {
        var item = LenientRetroPayException.NewLenientRetroPayException("code-997", "label-54");
        var view = LenientRetroPayExceptionView.ToLenientRetroPayExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientRetroPayExceptionView.ToLenientRetroPayExceptionViews(new List<LenientRetroPayException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientRetroPayExceptionViewShowsTheStatusInCapitals();
    }
}
