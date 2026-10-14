namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class FallbackBalanceMappingChecks
{
    public static void FallbackBalanceMappingStartsInOpen()
    {
        var item = FallbackBalanceMapping.NewFallbackBalanceMapping("code-150", "title-243");
        Check.Equal(item.Status, FallbackBalanceMappingStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackBalanceMappingMovesToResolved()
    {
        var item = FallbackBalanceMapping.NewFallbackBalanceMapping("code-150", "title-243");
        try
        {
            item.MoveTo(FallbackBalanceMappingStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackBalanceMappingStatus.Resolved, "status");
    }

    public static void FallbackBalanceMappingRejectsMovesFromArchived()
    {
        var item = FallbackBalanceMapping.NewFallbackBalanceMapping("code-150", "title-243");
        item.Status = FallbackBalanceMappingStatus.Archived;
        Check.Fails(() => item.MoveTo(FallbackBalanceMappingStatus.Open), "move");
    }

    public static void RunAll()
    {
        FallbackBalanceMappingStartsInOpen();
        FallbackBalanceMappingMovesToResolved();
        FallbackBalanceMappingRejectsMovesFromArchived();
    }
}
