namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RetroPayEntryChecks
{
    public static void RetroPayEntryStartsInOpen()
    {
        var item = RetroPayEntry.NewRetroPayEntry("code-729", "name-386");
        Check.Equal(item.Status, RetroPayEntryStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayEntryMovesToResolved()
    {
        var item = RetroPayEntry.NewRetroPayEntry("code-729", "name-386");
        try
        {
            item.MoveTo(RetroPayEntryStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayEntryStatus.Resolved, "status");
    }

    public static void RetroPayEntryRejectsMovesFromArchived()
    {
        var item = RetroPayEntry.NewRetroPayEntry("code-729", "name-386");
        item.Status = RetroPayEntryStatus.Archived;
        Check.Fails(() => item.MoveTo(RetroPayEntryStatus.Open), "move");
    }

    public static void RunAll()
    {
        RetroPayEntryStartsInOpen();
        RetroPayEntryMovesToResolved();
        RetroPayEntryRejectsMovesFromArchived();
    }
}
