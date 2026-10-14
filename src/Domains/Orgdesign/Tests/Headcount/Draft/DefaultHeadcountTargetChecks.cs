namespace Acme.Workforce.Orgdesign.Tests.Headcount.Draft;

using Acme.Workforce.Orgdesign.Headcount.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class DefaultHeadcountTargetChecks
{
    public static void DefaultHeadcountTargetStartsInProposed()
    {
        var item = DefaultHeadcountTarget.NewDefaultHeadcountTarget("code-230", "name-342");
        Check.Equal(item.Status, DefaultHeadcountTargetStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultHeadcountTargetMovesToApproved()
    {
        var item = DefaultHeadcountTarget.NewDefaultHeadcountTarget("code-230", "name-342");
        try
        {
            item.MoveTo(DefaultHeadcountTargetStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultHeadcountTargetStatus.Approved, "status");
    }

    public static void DefaultHeadcountTargetRejectsMovesFromWithdrawn()
    {
        var item = DefaultHeadcountTarget.NewDefaultHeadcountTarget("code-230", "name-342");
        item.Status = DefaultHeadcountTargetStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DefaultHeadcountTargetStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DefaultHeadcountTargetStartsInProposed();
        DefaultHeadcountTargetMovesToApproved();
        DefaultHeadcountTargetRejectsMovesFromWithdrawn();
    }
}
