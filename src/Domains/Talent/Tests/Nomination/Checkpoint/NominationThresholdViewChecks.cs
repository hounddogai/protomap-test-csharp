namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationThresholdViewChecks
{
    public static void NominationThresholdViewShowsTheStatusInCapitals()
    {
        var item = NominationThreshold.NewNominationThreshold("code-790", "label-872");
        var view = NominationThresholdView.ToNominationThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(NominationThresholdView.ToNominationThresholdViews(new List<NominationThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        NominationThresholdViewShowsTheStatusInCapitals();
    }
}
