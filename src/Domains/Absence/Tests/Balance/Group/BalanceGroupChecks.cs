namespace Acme.Workforce.Absence.Tests.Balance.Group;

using Acme.Workforce.Absence.Balance.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BalanceGroupChecks
{
    public static void BalanceGroupStartsInOpen()
    {
        var item = BalanceGroup.NewBalanceGroup("code-233", "label-372");
        Check.Equal(item.Status, BalanceGroupStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceGroupMovesToReopened()
    {
        var item = BalanceGroup.NewBalanceGroup("code-233", "label-372");
        try
        {
            item.MoveTo(BalanceGroupStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceGroupStatus.Reopened, "status");
    }

    public static void BalanceGroupRejectsMovesFromArchived()
    {
        var item = BalanceGroup.NewBalanceGroup("code-233", "label-372");
        item.Status = BalanceGroupStatus.Archived;
        Check.Fails(() => item.MoveTo(BalanceGroupStatus.Open), "move");
    }

    public static void RunAll()
    {
        BalanceGroupStartsInOpen();
        BalanceGroupMovesToReopened();
        BalanceGroupRejectsMovesFromArchived();
    }
}
