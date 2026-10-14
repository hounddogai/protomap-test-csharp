namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class NominationPolicyChecks
{
    public static void NominationPolicyStartsInRequested()
    {
        var item = NominationPolicy.NewNominationPolicy("code-334", "label-801");
        Check.Equal(item.Status, NominationPolicyStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationPolicyMovesToGranted()
    {
        var item = NominationPolicy.NewNominationPolicy("code-334", "label-801");
        try
        {
            item.MoveTo(NominationPolicyStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationPolicyStatus.Granted, "status");
    }

    public static void NominationPolicyRejectsMovesFromExpired()
    {
        var item = NominationPolicy.NewNominationPolicy("code-334", "label-801");
        item.Status = NominationPolicyStatus.Expired;
        Check.Fails(() => item.MoveTo(NominationPolicyStatus.Requested), "move");
    }

    public static void RunAll()
    {
        NominationPolicyStartsInRequested();
        NominationPolicyMovesToGranted();
        NominationPolicyRejectsMovesFromExpired();
    }
}
