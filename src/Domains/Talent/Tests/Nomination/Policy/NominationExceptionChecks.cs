namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class NominationExceptionChecks
{
    public static void NominationExceptionStartsInNew()
    {
        var item = NominationException.NewNominationException("code-371", "name-468");
        Check.Equal(item.Status, NominationExceptionStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationExceptionMovesToQueued()
    {
        var item = NominationException.NewNominationException("code-371", "name-468");
        try
        {
            item.MoveTo(NominationExceptionStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationExceptionStatus.Queued, "status");
    }

    public static void NominationExceptionRejectsMovesFromFailed()
    {
        var item = NominationException.NewNominationException("code-371", "name-468");
        item.Status = NominationExceptionStatus.Failed;
        Check.Fails(() => item.MoveTo(NominationExceptionStatus.New), "move");
    }

    public static void RunAll()
    {
        NominationExceptionStartsInNew();
        NominationExceptionMovesToQueued();
        NominationExceptionRejectsMovesFromFailed();
    }
}
