namespace Acme.Workforce.Orgdesign.Tests.Position.Template;

using Acme.Workforce.Orgdesign.Position.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PrimaryPositionProfileViewChecks
{
    public static void PrimaryPositionProfileViewShowsTheStatusInCapitals()
    {
        var item = PrimaryPositionProfile.NewPrimaryPositionProfile("code-880", "title-258");
        var view = PrimaryPositionProfileView.ToPrimaryPositionProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryPositionProfileView.ToPrimaryPositionProfileViews(new List<PrimaryPositionProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryPositionProfileViewShowsTheStatusInCapitals();
    }
}
