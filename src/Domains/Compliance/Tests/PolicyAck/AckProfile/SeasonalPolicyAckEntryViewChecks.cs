namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckProfile;

using Acme.Workforce.Compliance.PolicyAck.AckProfile;

/// <summary>Checks of ack profile code, run by the project's runner.</summary>
public static class SeasonalPolicyAckEntryViewChecks
{
    public static void SeasonalPolicyAckEntryViewShowsTheStatusInCapitals()
    {
        var item = SeasonalPolicyAckEntry.NewSeasonalPolicyAckEntry("code-887", "title-483");
        var view = SeasonalPolicyAckEntryView.ToSeasonalPolicyAckEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalPolicyAckEntryView.ToSeasonalPolicyAckEntryViews(new List<SeasonalPolicyAckEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalPolicyAckEntryViewShowsTheStatusInCapitals();
    }
}
