namespace Acme.Workforce.Orgdesign.Tests.Position.Entry;

using Acme.Workforce.Orgdesign.Position.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class RegionalPositionPolicyChecks
{
    public static void RegionalPositionPolicyStartsInProposed()
    {
        var item = RegionalPositionPolicy.NewRegionalPositionPolicy("code-28", "label-225");
        Check.Equal(item.Status, RegionalPositionPolicyStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalPositionPolicyMovesToApproved()
    {
        var item = RegionalPositionPolicy.NewRegionalPositionPolicy("code-28", "label-225");
        try
        {
            item.MoveTo(RegionalPositionPolicyStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalPositionPolicyStatus.Approved, "status");
    }

    public static void RegionalPositionPolicyRejectsMovesFromWithdrawn()
    {
        var item = RegionalPositionPolicy.NewRegionalPositionPolicy("code-28", "label-225");
        item.Status = RegionalPositionPolicyStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RegionalPositionPolicyStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RegionalPositionPolicyStartsInProposed();
        RegionalPositionPolicyMovesToApproved();
        RegionalPositionPolicyRejectsMovesFromWithdrawn();
    }
}
