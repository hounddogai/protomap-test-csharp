namespace Acme.Workforce.Talent.Tests.Nomination.Rollup;

using Acme.Workforce.Talent.Nomination.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class FallbackNominationExceptionChecks
{
    public static void FallbackNominationExceptionStartsInCreated()
    {
        var item = FallbackNominationException.NewFallbackNominationException("code-462", "name-787");
        Check.Equal(item.Status, FallbackNominationExceptionStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackNominationExceptionMovesToPublished()
    {
        var item = FallbackNominationException.NewFallbackNominationException("code-462", "name-787");
        try
        {
            item.MoveTo(FallbackNominationExceptionStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackNominationExceptionStatus.Published, "status");
    }

    public static void FallbackNominationExceptionRejectsMovesFromRetired()
    {
        var item = FallbackNominationException.NewFallbackNominationException("code-462", "name-787");
        item.Status = FallbackNominationExceptionStatus.Retired;
        Check.Fails(() => item.MoveTo(FallbackNominationExceptionStatus.Created), "move");
    }

    public static void RunAll()
    {
        FallbackNominationExceptionStartsInCreated();
        FallbackNominationExceptionMovesToPublished();
        FallbackNominationExceptionRejectsMovesFromRetired();
    }
}
