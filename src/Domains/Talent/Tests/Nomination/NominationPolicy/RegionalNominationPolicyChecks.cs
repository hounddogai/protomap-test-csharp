namespace Acme.Workforce.Talent.Tests.Nomination.NominationPolicy;

using Acme.Workforce.Talent.Nomination.NominationPolicy;

/// <summary>Checks of nomination policy code, run by the project's runner.</summary>
public static class RegionalNominationPolicyChecks
{
    public static void RegionalNominationPolicyStartsInIdle()
    {
        var item = RegionalNominationPolicy.NewRegionalNominationPolicy("code-835", "name-358");
        Check.Equal(item.Status, RegionalNominationPolicyStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalNominationPolicyMovesToDraining()
    {
        var item = RegionalNominationPolicy.NewRegionalNominationPolicy("code-835", "name-358");
        try
        {
            item.MoveTo(RegionalNominationPolicyStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalNominationPolicyStatus.Draining, "status");
    }

    public static void RegionalNominationPolicyRejectsMovesFromStopped()
    {
        var item = RegionalNominationPolicy.NewRegionalNominationPolicy("code-835", "name-358");
        item.Status = RegionalNominationPolicyStatus.Stopped;
        Check.Fails(() => item.MoveTo(RegionalNominationPolicyStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RegionalNominationPolicyStartsInIdle();
        RegionalNominationPolicyMovesToDraining();
        RegionalNominationPolicyRejectsMovesFromStopped();
    }
}
