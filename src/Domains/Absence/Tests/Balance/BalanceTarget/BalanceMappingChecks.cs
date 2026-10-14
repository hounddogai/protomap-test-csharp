namespace Acme.Workforce.Absence.Tests.Balance.BalanceTarget;

using Acme.Workforce.Absence.Balance.BalanceTarget;

/// <summary>Checks of balance target code, run by the project's runner.</summary>
public static class BalanceMappingChecks
{
    public static void BalanceMappingStartsInOpen()
    {
        var item = BalanceMapping.NewBalanceMapping("code-102", "title-283");
        Check.Equal(item.Status, BalanceMappingStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceMappingMovesToInReview()
    {
        var item = BalanceMapping.NewBalanceMapping("code-102", "title-283");
        try
        {
            item.MoveTo(BalanceMappingStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceMappingStatus.InReview, "status");
    }

    public static void BalanceMappingRejectsMovesFromArchived()
    {
        var item = BalanceMapping.NewBalanceMapping("code-102", "title-283");
        item.Status = BalanceMappingStatus.Archived;
        Check.Fails(() => item.MoveTo(BalanceMappingStatus.Open), "move");
    }

    public static void RunAll()
    {
        BalanceMappingStartsInOpen();
        BalanceMappingMovesToInReview();
        BalanceMappingRejectsMovesFromArchived();
    }
}
