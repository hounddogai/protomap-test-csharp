namespace Acme.Workforce.Talent.Tests.Mobility.Manifest;

using Acme.Workforce.Talent.Mobility.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ScheduledMobilityPreferenceChecks
{
    public static void ScheduledMobilityPreferenceStartsInDraft()
    {
        var item = ScheduledMobilityPreference.NewScheduledMobilityPreference("code-172", "label-528");
        Check.Equal(item.Status, ScheduledMobilityPreferenceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledMobilityPreferenceMovesToActive()
    {
        var item = ScheduledMobilityPreference.NewScheduledMobilityPreference("code-172", "label-528");
        try
        {
            item.MoveTo(ScheduledMobilityPreferenceStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledMobilityPreferenceStatus.Active, "status");
    }

    public static void ScheduledMobilityPreferenceRejectsMovesFromClosed()
    {
        var item = ScheduledMobilityPreference.NewScheduledMobilityPreference("code-172", "label-528");
        item.Status = ScheduledMobilityPreferenceStatus.Closed;
        Check.Fails(() => item.MoveTo(ScheduledMobilityPreferenceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ScheduledMobilityPreferenceStartsInDraft();
        ScheduledMobilityPreferenceMovesToActive();
        ScheduledMobilityPreferenceRejectsMovesFromClosed();
    }
}
