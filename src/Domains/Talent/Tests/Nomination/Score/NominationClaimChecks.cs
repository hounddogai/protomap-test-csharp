namespace Acme.Workforce.Talent.Tests.Nomination.Score;

using Acme.Workforce.Talent.Nomination.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class NominationClaimChecks
{
    public static void NominationClaimStartsInProposed()
    {
        var item = NominationClaim.NewNominationClaim("code-256", "title-307");
        Check.Equal(item.Status, NominationClaimStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationClaimMovesToWithdrawn()
    {
        var item = NominationClaim.NewNominationClaim("code-256", "title-307");
        try
        {
            item.MoveTo(NominationClaimStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationClaimStatus.Withdrawn, "status");
    }

    public static void NominationClaimRejectsMovesFromWithdrawn()
    {
        var item = NominationClaim.NewNominationClaim("code-256", "title-307");
        item.Status = NominationClaimStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(NominationClaimStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        NominationClaimStartsInProposed();
        NominationClaimMovesToWithdrawn();
        NominationClaimRejectsMovesFromWithdrawn();
    }
}
