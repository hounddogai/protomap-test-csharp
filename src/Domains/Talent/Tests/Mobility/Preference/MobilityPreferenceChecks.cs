namespace Acme.Workforce.Talent.Tests.Mobility.Preference;

using Acme.Workforce.Talent.Mobility.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class MobilityPreferenceChecks
{
    public static void MobilityPreferenceStartsInOpen()
    {
        var item = MobilityPreference.NewMobilityPreference("code-771", "title-41");
        Check.Equal(item.Status, MobilityPreferenceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityPreferenceMovesToInReview()
    {
        var item = MobilityPreference.NewMobilityPreference("code-771", "title-41");
        try
        {
            item.MoveTo(MobilityPreferenceStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityPreferenceStatus.InReview, "status");
    }

    public static void MobilityPreferenceRejectsMovesFromArchived()
    {
        var item = MobilityPreference.NewMobilityPreference("code-771", "title-41");
        item.Status = MobilityPreferenceStatus.Archived;
        Check.Fails(() => item.MoveTo(MobilityPreferenceStatus.Open), "move");
    }

    public static void RunAll()
    {
        MobilityPreferenceStartsInOpen();
        MobilityPreferenceMovesToInReview();
        MobilityPreferenceRejectsMovesFromArchived();
    }
}
