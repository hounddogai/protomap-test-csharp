namespace Acme.Workforce.Payrollops.Tests.RetroPay.Draft;

using Acme.Workforce.Payrollops.RetroPay.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RetroPayDraftChecks
{
    public static void RetroPayDraftStartsInCreated()
    {
        var item = RetroPayDraft.NewRetroPayDraft("code-509", "title-195");
        Check.Equal(item.Status, RetroPayDraftStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayDraftMovesToValidated()
    {
        var item = RetroPayDraft.NewRetroPayDraft("code-509", "title-195");
        try
        {
            item.MoveTo(RetroPayDraftStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayDraftStatus.Validated, "status");
    }

    public static void RetroPayDraftRejectsMovesFromRetired()
    {
        var item = RetroPayDraft.NewRetroPayDraft("code-509", "title-195");
        item.Status = RetroPayDraftStatus.Retired;
        Check.Fails(() => item.MoveTo(RetroPayDraftStatus.Created), "move");
    }

    public static void RunAll()
    {
        RetroPayDraftStartsInCreated();
        RetroPayDraftMovesToValidated();
        RetroPayDraftRejectsMovesFromRetired();
    }
}
