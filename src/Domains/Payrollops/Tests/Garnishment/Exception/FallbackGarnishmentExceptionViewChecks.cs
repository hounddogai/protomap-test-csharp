namespace Acme.Workforce.Payrollops.Tests.Garnishment.Exception;

using Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class FallbackGarnishmentExceptionViewChecks
{
    public static void FallbackGarnishmentExceptionViewShowsTheStatusInCapitals()
    {
        var item = FallbackGarnishmentException.NewFallbackGarnishmentException("code-942", "name-868");
        var view = FallbackGarnishmentExceptionView.ToFallbackGarnishmentExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackGarnishmentExceptionView.ToFallbackGarnishmentExceptionViews(new List<FallbackGarnishmentException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackGarnishmentExceptionViewShowsTheStatusInCapitals();
    }
}
