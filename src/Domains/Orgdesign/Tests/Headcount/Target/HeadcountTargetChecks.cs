namespace Acme.Workforce.Orgdesign.Tests.Headcount.Target;

using Acme.Workforce.Orgdesign.Headcount.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class HeadcountTargetChecks
{
    public static void HeadcountTargetStartsInProposed()
    {
        var item = HeadcountTarget.NewHeadcountTarget("code-22", "label-480");
        Check.Equal(item.Status, HeadcountTargetStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountTargetMovesToApproved()
    {
        var item = HeadcountTarget.NewHeadcountTarget("code-22", "label-480");
        try
        {
            item.MoveTo(HeadcountTargetStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountTargetStatus.Approved, "status");
    }

    public static void HeadcountTargetRejectsMovesFromWithdrawn()
    {
        var item = HeadcountTarget.NewHeadcountTarget("code-22", "label-480");
        item.Status = HeadcountTargetStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(HeadcountTargetStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        HeadcountTargetStartsInProposed();
        HeadcountTargetMovesToApproved();
        HeadcountTargetRejectsMovesFromWithdrawn();
    }
}
