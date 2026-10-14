namespace Acme.Workforce.Compensation.Tests.MeritCycle.Profile;

using Acme.Workforce.Compensation.MeritCycle.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class MeritCycleProfileViewChecks
{
    public static void MeritCycleProfileViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleProfile.NewMeritCycleProfile("code-538", "title-918");
        var view = MeritCycleProfileView.ToMeritCycleProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleProfileView.ToMeritCycleProfileViews(new List<MeritCycleProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleProfileViewShowsTheStatusInCapitals();
    }
}
