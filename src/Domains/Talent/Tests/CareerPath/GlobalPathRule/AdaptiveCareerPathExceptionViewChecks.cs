namespace Acme.Workforce.Talent.Tests.CareerPath.GlobalPathRule;

using Acme.Workforce.Talent.CareerPath.GlobalPathRule;

/// <summary>Checks of global path rule code, run by the project's runner.</summary>
public static class AdaptiveCareerPathExceptionViewChecks
{
    public static void AdaptiveCareerPathExceptionViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveCareerPathException.NewAdaptiveCareerPathException("code-163", "label-535");
        var view = AdaptiveCareerPathExceptionView.ToAdaptiveCareerPathExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveCareerPathExceptionView.ToAdaptiveCareerPathExceptionViews(new List<AdaptiveCareerPathException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveCareerPathExceptionViewShowsTheStatusInCapitals();
    }
}
