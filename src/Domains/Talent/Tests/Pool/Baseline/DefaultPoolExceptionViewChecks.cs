namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DefaultPoolExceptionViewChecks
{
    public static void DefaultPoolExceptionViewShowsTheStatusInCapitals()
    {
        var item = DefaultPoolException.NewDefaultPoolException("code-237", "title-233");
        var view = DefaultPoolExceptionView.ToDefaultPoolExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultPoolExceptionView.ToDefaultPoolExceptionViews(new List<DefaultPoolException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultPoolExceptionViewShowsTheStatusInCapitals();
    }
}
