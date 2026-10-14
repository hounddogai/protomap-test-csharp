namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class ExceptionClaimViewChecks
{
    public static void ExceptionClaimViewShowsTheStatusInCapitals()
    {
        var item = ExceptionClaim.NewExceptionClaim("code-868", "name-797");
        var view = ExceptionClaimView.ToExceptionClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionClaimView.ToExceptionClaimViews(new List<ExceptionClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionClaimViewShowsTheStatusInCapitals();
    }
}
