namespace Acme.Workforce.Talent.Tests.Nomination.Rollup;

using Acme.Workforce.Talent.Nomination.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class FallbackNominationExceptionViewChecks
{
    public static void FallbackNominationExceptionViewShowsTheStatusInCapitals()
    {
        var item = FallbackNominationException.NewFallbackNominationException("code-462", "name-787");
        var view = FallbackNominationExceptionView.ToFallbackNominationExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackNominationExceptionView.ToFallbackNominationExceptionViews(new List<FallbackNominationException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackNominationExceptionViewShowsTheStatusInCapitals();
    }
}
