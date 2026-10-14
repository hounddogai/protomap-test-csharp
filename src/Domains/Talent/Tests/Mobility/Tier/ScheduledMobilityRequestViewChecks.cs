namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ScheduledMobilityRequestViewChecks
{
    public static void ScheduledMobilityRequestViewShowsTheStatusInCapitals()
    {
        var item = ScheduledMobilityRequest.NewScheduledMobilityRequest("code-289", "name-650");
        var view = ScheduledMobilityRequestView.ToScheduledMobilityRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledMobilityRequestView.ToScheduledMobilityRequestViews(new List<ScheduledMobilityRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledMobilityRequestViewShowsTheStatusInCapitals();
    }
}
