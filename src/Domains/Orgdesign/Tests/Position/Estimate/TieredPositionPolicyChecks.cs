namespace Acme.Workforce.Orgdesign.Tests.Position.Estimate;

using Acme.Workforce.Orgdesign.Position.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TieredPositionPolicyChecks
{
    public static void TieredPositionPolicyStartsInProposed()
    {
        var item = TieredPositionPolicy.NewTieredPositionPolicy("code-994", "title-522");
        Check.Equal(item.Status, TieredPositionPolicyStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPositionPolicyMovesToApproved()
    {
        var item = TieredPositionPolicy.NewTieredPositionPolicy("code-994", "title-522");
        try
        {
            item.MoveTo(TieredPositionPolicyStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPositionPolicyStatus.Approved, "status");
    }

    public static void TieredPositionPolicyRejectsMovesFromWithdrawn()
    {
        var item = TieredPositionPolicy.NewTieredPositionPolicy("code-994", "title-522");
        item.Status = TieredPositionPolicyStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TieredPositionPolicyStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TieredPositionPolicyStartsInProposed();
        TieredPositionPolicyMovesToApproved();
        TieredPositionPolicyRejectsMovesFromWithdrawn();
    }
}
