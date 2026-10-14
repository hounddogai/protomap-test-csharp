namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DefaultPolicyVarianceChecks
{
    public static void DefaultPolicyVarianceStartsInDraft()
    {
        var item = DefaultPolicyVariance.NewDefaultPolicyVariance("code-663", "label-130");
        Check.Equal(item.Status, DefaultPolicyVarianceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPolicyVarianceMovesToSuspended()
    {
        var item = DefaultPolicyVariance.NewDefaultPolicyVariance("code-663", "label-130");
        try
        {
            item.MoveTo(DefaultPolicyVarianceStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPolicyVarianceStatus.Suspended, "status");
    }

    public static void DefaultPolicyVarianceRejectsMovesFromClosed()
    {
        var item = DefaultPolicyVariance.NewDefaultPolicyVariance("code-663", "label-130");
        item.Status = DefaultPolicyVarianceStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultPolicyVarianceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultPolicyVarianceStartsInDraft();
        DefaultPolicyVarianceMovesToSuspended();
        DefaultPolicyVarianceRejectsMovesFromClosed();
    }
}
