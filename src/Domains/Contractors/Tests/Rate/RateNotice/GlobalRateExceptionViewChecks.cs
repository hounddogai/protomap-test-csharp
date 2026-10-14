namespace Acme.Workforce.Contractors.Tests.Rate.RateNotice;

using Acme.Workforce.Contractors.Rate.RateNotice;

/// <summary>Checks of rate notice code, run by the project's runner.</summary>
public static class GlobalRateExceptionViewChecks
{
    public static void GlobalRateExceptionViewShowsTheStatusInCapitals()
    {
        var item = GlobalRateException.NewGlobalRateException("code-708", "label-551");
        var view = GlobalRateExceptionView.ToGlobalRateExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalRateExceptionView.ToGlobalRateExceptionViews(new List<GlobalRateException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalRateExceptionViewShowsTheStatusInCapitals();
    }
}
