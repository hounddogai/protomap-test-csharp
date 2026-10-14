namespace Acme.Workforce.Compliance.Tests.Exception.Plan;

using Acme.Workforce.Compliance.Exception.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PrimaryExceptionRequestChecks
{
    public static void PrimaryExceptionRequestStartsInDraft()
    {
        var item = PrimaryExceptionRequest.NewPrimaryExceptionRequest("code-106", "title-469");
        Check.Equal(item.Status, PrimaryExceptionRequestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryExceptionRequestMovesToPending()
    {
        var item = PrimaryExceptionRequest.NewPrimaryExceptionRequest("code-106", "title-469");
        try
        {
            item.MoveTo(PrimaryExceptionRequestStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryExceptionRequestStatus.Pending, "status");
    }

    public static void PrimaryExceptionRequestRejectsMovesFromClosed()
    {
        var item = PrimaryExceptionRequest.NewPrimaryExceptionRequest("code-106", "title-469");
        item.Status = PrimaryExceptionRequestStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryExceptionRequestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryExceptionRequestStartsInDraft();
        PrimaryExceptionRequestMovesToPending();
        PrimaryExceptionRequestRejectsMovesFromClosed();
    }
}
