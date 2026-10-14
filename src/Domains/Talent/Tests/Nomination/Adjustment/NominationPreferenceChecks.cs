namespace Acme.Workforce.Talent.Tests.Nomination.Adjustment;

using Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class NominationPreferenceChecks
{
    public static void NominationPreferenceStartsInIdle()
    {
        var item = NominationPreference.NewNominationPreference("code-805", "name-754");
        Check.Equal(item.Status, NominationPreferenceStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationPreferenceMovesToStopped()
    {
        var item = NominationPreference.NewNominationPreference("code-805", "name-754");
        try
        {
            item.MoveTo(NominationPreferenceStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationPreferenceStatus.Stopped, "status");
    }

    public static void NominationPreferenceRejectsMovesFromStopped()
    {
        var item = NominationPreference.NewNominationPreference("code-805", "name-754");
        item.Status = NominationPreferenceStatus.Stopped;
        Check.Fails(() => item.MoveTo(NominationPreferenceStatus.Idle), "move");
    }

    public static void RunAll()
    {
        NominationPreferenceStartsInIdle();
        NominationPreferenceMovesToStopped();
        NominationPreferenceRejectsMovesFromStopped();
    }
}
