namespace Acme.Workforce.Talent.Tests.CareerPath.PathOverride;

using Acme.Workforce.Talent.CareerPath.PathOverride;

/// <summary>Checks of path override code, run by the project's runner.</summary>
public static class DefaultCareerPathExceptionViewChecks
{
    public static void DefaultCareerPathExceptionViewShowsTheStatusInCapitals()
    {
        var item = DefaultCareerPathException.NewDefaultCareerPathException("code-352", "label-131");
        var view = DefaultCareerPathExceptionView.ToDefaultCareerPathExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultCareerPathExceptionView.ToDefaultCareerPathExceptionViews(new List<DefaultCareerPathException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultCareerPathExceptionViewShowsTheStatusInCapitals();
    }
}
