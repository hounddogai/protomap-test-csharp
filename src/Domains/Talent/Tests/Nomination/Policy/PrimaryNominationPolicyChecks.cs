namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PrimaryNominationPolicyChecks
{
    public static void PrimaryNominationPolicyStartsInIdle()
    {
        var item = PrimaryNominationPolicy.NewPrimaryNominationPolicy("code-319", "title-30");
        Check.Equal(item.Status, PrimaryNominationPolicyStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryNominationPolicyMovesToReady()
    {
        var item = PrimaryNominationPolicy.NewPrimaryNominationPolicy("code-319", "title-30");
        try
        {
            item.MoveTo(PrimaryNominationPolicyStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryNominationPolicyStatus.Ready, "status");
    }

    public static void PrimaryNominationPolicyRejectsMovesFromStopped()
    {
        var item = PrimaryNominationPolicy.NewPrimaryNominationPolicy("code-319", "title-30");
        item.Status = PrimaryNominationPolicyStatus.Stopped;
        Check.Fails(() => item.MoveTo(PrimaryNominationPolicyStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PrimaryNominationPolicyStartsInIdle();
        PrimaryNominationPolicyMovesToReady();
        PrimaryNominationPolicyRejectsMovesFromStopped();
    }
}
