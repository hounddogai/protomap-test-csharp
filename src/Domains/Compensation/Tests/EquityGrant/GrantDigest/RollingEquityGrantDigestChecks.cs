namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantDigest;

using Acme.Workforce.Compensation.EquityGrant.GrantDigest;

/// <summary>Checks of grant digest code, run by the project's runner.</summary>
public static class RollingEquityGrantDigestChecks
{
    public static void RollingEquityGrantDigestStartsInProposed()
    {
        var item = RollingEquityGrantDigest.NewRollingEquityGrantDigest("code-254", "label-418");
        Check.Equal(item.Status, RollingEquityGrantDigestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingEquityGrantDigestMovesToApproved()
    {
        var item = RollingEquityGrantDigest.NewRollingEquityGrantDigest("code-254", "label-418");
        try
        {
            item.MoveTo(RollingEquityGrantDigestStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingEquityGrantDigestStatus.Approved, "status");
    }

    public static void RollingEquityGrantDigestRejectsMovesFromWithdrawn()
    {
        var item = RollingEquityGrantDigest.NewRollingEquityGrantDigest("code-254", "label-418");
        item.Status = RollingEquityGrantDigestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RollingEquityGrantDigestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RollingEquityGrantDigestStartsInProposed();
        RollingEquityGrantDigestMovesToApproved();
        RollingEquityGrantDigestRejectsMovesFromWithdrawn();
    }
}
