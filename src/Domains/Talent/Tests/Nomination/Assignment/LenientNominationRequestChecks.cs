namespace Acme.Workforce.Talent.Tests.Nomination.Assignment;

using Acme.Workforce.Talent.Nomination.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LenientNominationRequestChecks
{
    public static void LenientNominationRequestStartsInCreated()
    {
        var item = LenientNominationRequest.NewLenientNominationRequest("code-215", "label-56");
        Check.Equal(item.Status, LenientNominationRequestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientNominationRequestMovesToPublished()
    {
        var item = LenientNominationRequest.NewLenientNominationRequest("code-215", "label-56");
        try
        {
            item.MoveTo(LenientNominationRequestStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientNominationRequestStatus.Published, "status");
    }

    public static void LenientNominationRequestRejectsMovesFromRetired()
    {
        var item = LenientNominationRequest.NewLenientNominationRequest("code-215", "label-56");
        item.Status = LenientNominationRequestStatus.Retired;
        Check.Fails(() => item.MoveTo(LenientNominationRequestStatus.Created), "move");
    }

    public static void RunAll()
    {
        LenientNominationRequestStartsInCreated();
        LenientNominationRequestMovesToPublished();
        LenientNominationRequestRejectsMovesFromRetired();
    }
}
