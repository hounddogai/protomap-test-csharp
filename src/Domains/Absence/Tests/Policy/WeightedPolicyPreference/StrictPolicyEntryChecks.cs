namespace Acme.Workforce.Absence.Tests.Policy.WeightedPolicyPreference;

using Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

/// <summary>Checks of weighted policy preference code, run by the project's runner.</summary>
public static class StrictPolicyEntryChecks
{
    public static void StrictPolicyEntryStartsInRequested()
    {
        var item = StrictPolicyEntry.NewStrictPolicyEntry("code-352", "name-282");
        Check.Equal(item.Status, StrictPolicyEntryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPolicyEntryMovesToRevoked()
    {
        var item = StrictPolicyEntry.NewStrictPolicyEntry("code-352", "name-282");
        try
        {
            item.MoveTo(StrictPolicyEntryStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPolicyEntryStatus.Revoked, "status");
    }

    public static void StrictPolicyEntryRejectsMovesFromExpired()
    {
        var item = StrictPolicyEntry.NewStrictPolicyEntry("code-352", "name-282");
        item.Status = StrictPolicyEntryStatus.Expired;
        Check.Fails(() => item.MoveTo(StrictPolicyEntryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StrictPolicyEntryStartsInRequested();
        StrictPolicyEntryMovesToRevoked();
        StrictPolicyEntryRejectsMovesFromExpired();
    }
}
