namespace Acme.Workforce.Absence.Tests.Policy.Preference;

using Acme.Workforce.Absence.Policy.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PolicyPreferenceChecks
{
    public static void PolicyPreferenceStartsInProposed()
    {
        var item = PolicyPreference.NewPolicyPreference("code-879", "title-426");
        Check.Equal(item.Status, PolicyPreferenceStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyPreferenceMovesToApproved()
    {
        var item = PolicyPreference.NewPolicyPreference("code-879", "title-426");
        try
        {
            item.MoveTo(PolicyPreferenceStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyPreferenceStatus.Approved, "status");
    }

    public static void PolicyPreferenceRejectsMovesFromWithdrawn()
    {
        var item = PolicyPreference.NewPolicyPreference("code-879", "title-426");
        item.Status = PolicyPreferenceStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PolicyPreferenceStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PolicyPreferenceStartsInProposed();
        PolicyPreferenceMovesToApproved();
        PolicyPreferenceRejectsMovesFromWithdrawn();
    }
}
