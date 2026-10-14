namespace Acme.Workforce.Absence.Tests.Balance.DefaultBalanceGroup;

using Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

/// <summary>Checks of default balance group code, run by the project's runner.</summary>
public static class LenientBalanceGroupChecks
{
    public static void LenientBalanceGroupStartsInCreated()
    {
        var item = LenientBalanceGroup.NewLenientBalanceGroup("code-263", "name-803");
        Check.Equal(item.Status, LenientBalanceGroupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientBalanceGroupMovesToValidated()
    {
        var item = LenientBalanceGroup.NewLenientBalanceGroup("code-263", "name-803");
        try
        {
            item.MoveTo(LenientBalanceGroupStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientBalanceGroupStatus.Validated, "status");
    }

    public static void LenientBalanceGroupRejectsMovesFromRetired()
    {
        var item = LenientBalanceGroup.NewLenientBalanceGroup("code-263", "name-803");
        item.Status = LenientBalanceGroupStatus.Retired;
        Check.Fails(() => item.MoveTo(LenientBalanceGroupStatus.Created), "move");
    }

    public static void RunAll()
    {
        LenientBalanceGroupStartsInCreated();
        LenientBalanceGroupMovesToValidated();
        LenientBalanceGroupRejectsMovesFromRetired();
    }
}
