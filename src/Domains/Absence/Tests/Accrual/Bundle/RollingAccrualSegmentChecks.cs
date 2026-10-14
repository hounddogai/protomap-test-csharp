namespace Acme.Workforce.Absence.Tests.Accrual.Bundle;

using Acme.Workforce.Absence.Accrual.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class RollingAccrualSegmentChecks
{
    public static void RollingAccrualSegmentStartsInProposed()
    {
        var item = RollingAccrualSegment.NewRollingAccrualSegment("code-128", "title-893");
        Check.Equal(item.Status, RollingAccrualSegmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingAccrualSegmentMovesToApproved()
    {
        var item = RollingAccrualSegment.NewRollingAccrualSegment("code-128", "title-893");
        try
        {
            item.MoveTo(RollingAccrualSegmentStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingAccrualSegmentStatus.Approved, "status");
    }

    public static void RollingAccrualSegmentRejectsMovesFromWithdrawn()
    {
        var item = RollingAccrualSegment.NewRollingAccrualSegment("code-128", "title-893");
        item.Status = RollingAccrualSegmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RollingAccrualSegmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RollingAccrualSegmentStartsInProposed();
        RollingAccrualSegmentMovesToApproved();
        RollingAccrualSegmentRejectsMovesFromWithdrawn();
    }
}
