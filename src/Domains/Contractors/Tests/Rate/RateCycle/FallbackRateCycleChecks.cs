namespace Acme.Workforce.Contractors.Tests.Rate.RateCycle;

using Acme.Workforce.Contractors.Rate.RateCycle;

/// <summary>Checks of rate cycle code, run by the project's runner.</summary>
public static class FallbackRateCycleChecks
{
    public static void FallbackRateCycleStartsInCreated()
    {
        var item = FallbackRateCycle.NewFallbackRateCycle("code-623", "name-244");
        Check.Equal(item.Status, FallbackRateCycleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackRateCycleMovesToPublished()
    {
        var item = FallbackRateCycle.NewFallbackRateCycle("code-623", "name-244");
        try
        {
            item.MoveTo(FallbackRateCycleStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackRateCycleStatus.Published, "status");
    }

    public static void FallbackRateCycleRejectsMovesFromRetired()
    {
        var item = FallbackRateCycle.NewFallbackRateCycle("code-623", "name-244");
        item.Status = FallbackRateCycleStatus.Retired;
        Check.Fails(() => item.MoveTo(FallbackRateCycleStatus.Created), "move");
    }

    public static void RunAll()
    {
        FallbackRateCycleStartsInCreated();
        FallbackRateCycleMovesToPublished();
        FallbackRateCycleRejectsMovesFromRetired();
    }
}
