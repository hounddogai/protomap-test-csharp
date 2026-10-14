namespace Acme.Workforce.Workplace.Tests.Desk.Allocation;

using Acme.Workforce.Workplace.Desk.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class DeskPolicyChecks
{
    public static void DeskPolicyStartsInNew()
    {
        var item = DeskPolicy.NewDeskPolicy("code-1", "title-190");
        Check.Equal(item.Status, DeskPolicyStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskPolicyMovesToFailed()
    {
        var item = DeskPolicy.NewDeskPolicy("code-1", "title-190");
        try
        {
            item.MoveTo(DeskPolicyStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskPolicyStatus.Failed, "status");
    }

    public static void DeskPolicyRejectsMovesFromFailed()
    {
        var item = DeskPolicy.NewDeskPolicy("code-1", "title-190");
        item.Status = DeskPolicyStatus.Failed;
        Check.Fails(() => item.MoveTo(DeskPolicyStatus.New), "move");
    }

    public static void RunAll()
    {
        DeskPolicyStartsInNew();
        DeskPolicyMovesToFailed();
        DeskPolicyRejectsMovesFromFailed();
    }
}
