namespace Acme.Workforce.Talent.Tests.Nomination.Request;

using Acme.Workforce.Talent.Nomination.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class FallbackNominationBundleChecks
{
    public static void FallbackNominationBundleStartsInProposed()
    {
        var item = FallbackNominationBundle.NewFallbackNominationBundle("code-239", "title-29");
        Check.Equal(item.Status, FallbackNominationBundleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackNominationBundleMovesToApproved()
    {
        var item = FallbackNominationBundle.NewFallbackNominationBundle("code-239", "title-29");
        try
        {
            item.MoveTo(FallbackNominationBundleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackNominationBundleStatus.Approved, "status");
    }

    public static void FallbackNominationBundleRejectsMovesFromWithdrawn()
    {
        var item = FallbackNominationBundle.NewFallbackNominationBundle("code-239", "title-29");
        item.Status = FallbackNominationBundleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FallbackNominationBundleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FallbackNominationBundleStartsInProposed();
        FallbackNominationBundleMovesToApproved();
        FallbackNominationBundleRejectsMovesFromWithdrawn();
    }
}
