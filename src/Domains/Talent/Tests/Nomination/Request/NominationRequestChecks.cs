namespace Acme.Workforce.Talent.Tests.Nomination.Request;

using Acme.Workforce.Talent.Nomination.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class NominationRequestChecks
{
    public static void NominationRequestStartsInNew()
    {
        var item = NominationRequest.NewNominationRequest("code-32", "title-560");
        Check.Equal(item.Status, NominationRequestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationRequestMovesToDone()
    {
        var item = NominationRequest.NewNominationRequest("code-32", "title-560");
        try
        {
            item.MoveTo(NominationRequestStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationRequestStatus.Done, "status");
    }

    public static void NominationRequestRejectsMovesFromFailed()
    {
        var item = NominationRequest.NewNominationRequest("code-32", "title-560");
        item.Status = NominationRequestStatus.Failed;
        Check.Fails(() => item.MoveTo(NominationRequestStatus.New), "move");
    }

    public static void RunAll()
    {
        NominationRequestStartsInNew();
        NominationRequestMovesToDone();
        NominationRequestRejectsMovesFromFailed();
    }
}
