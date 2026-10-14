namespace Acme.Workforce.Absence.Tests.Balance.Checkpoint;

using Acme.Workforce.Absence.Balance.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class BalanceTemplateChecks
{
    public static void BalanceTemplateStartsInRequested()
    {
        var item = BalanceTemplate.NewBalanceTemplate("code-301", "name-924");
        Check.Equal(item.Status, BalanceTemplateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceTemplateMovesToGranted()
    {
        var item = BalanceTemplate.NewBalanceTemplate("code-301", "name-924");
        try
        {
            item.MoveTo(BalanceTemplateStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceTemplateStatus.Granted, "status");
    }

    public static void BalanceTemplateRejectsMovesFromExpired()
    {
        var item = BalanceTemplate.NewBalanceTemplate("code-301", "name-924");
        item.Status = BalanceTemplateStatus.Expired;
        Check.Fails(() => item.MoveTo(BalanceTemplateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BalanceTemplateStartsInRequested();
        BalanceTemplateMovesToGranted();
        BalanceTemplateRejectsMovesFromExpired();
    }
}
