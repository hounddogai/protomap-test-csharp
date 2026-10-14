namespace Acme.Workforce.Orgdesign.Tests.Position.Audit;

using Acme.Workforce.Orgdesign.Position.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AdaptivePositionProfileViewChecks
{
    public static void AdaptivePositionProfileViewShowsTheStatusInCapitals()
    {
        var item = AdaptivePositionProfile.NewAdaptivePositionProfile("code-320", "title-450");
        var view = AdaptivePositionProfileView.ToAdaptivePositionProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptivePositionProfileView.ToAdaptivePositionProfileViews(new List<AdaptivePositionProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptivePositionProfileViewShowsTheStatusInCapitals();
    }
}
