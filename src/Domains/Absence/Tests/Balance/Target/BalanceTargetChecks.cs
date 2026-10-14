namespace Acme.Workforce.Absence.Tests.Balance.Target;

using Acme.Workforce.Absence.Balance.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class BalanceTargetChecks
{
    public static void BalanceTargetStartsInOpen()
    {
        var item = BalanceTarget.NewBalanceTarget("code-89", "name-233");
        Check.Equal(item.Status, BalanceTargetStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceTargetMovesToInReview()
    {
        var item = BalanceTarget.NewBalanceTarget("code-89", "name-233");
        try
        {
            item.MoveTo(BalanceTargetStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceTargetStatus.InReview, "status");
    }

    public static void BalanceTargetRejectsMovesFromArchived()
    {
        var item = BalanceTarget.NewBalanceTarget("code-89", "name-233");
        item.Status = BalanceTargetStatus.Archived;
        Check.Fails(() => item.MoveTo(BalanceTargetStatus.Open), "move");
    }

    public static void RunAll()
    {
        BalanceTargetStartsInOpen();
        BalanceTargetMovesToInReview();
        BalanceTargetRejectsMovesFromArchived();
    }
}
