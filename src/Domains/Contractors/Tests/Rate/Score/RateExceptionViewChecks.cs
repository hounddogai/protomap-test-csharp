namespace Acme.Workforce.Contractors.Tests.Rate.Score;

using Acme.Workforce.Contractors.Rate.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class RateExceptionViewChecks
{
    public static void RateExceptionViewShowsTheStatusInCapitals()
    {
        var item = RateException.NewRateException("code-18", "title-77");
        var view = RateExceptionView.ToRateExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RateExceptionView.ToRateExceptionViews(new List<RateException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RateExceptionViewShowsTheStatusInCapitals();
    }
}
