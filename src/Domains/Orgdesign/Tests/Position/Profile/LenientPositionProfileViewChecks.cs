namespace Acme.Workforce.Orgdesign.Tests.Position.Profile;

using Acme.Workforce.Orgdesign.Position.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class LenientPositionProfileViewChecks
{
    public static void LenientPositionProfileViewShowsTheStatusInCapitals()
    {
        var item = LenientPositionProfile.NewLenientPositionProfile("code-476", "title-553");
        var view = LenientPositionProfileView.ToLenientPositionProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientPositionProfileView.ToLenientPositionProfileViews(new List<LenientPositionProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientPositionProfileViewShowsTheStatusInCapitals();
    }
}
