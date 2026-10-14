namespace Acme.Workforce.Contractors.Tests.Rate.Bundle;

using Acme.Workforce.Contractors.Rate.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FallbackRateAllocationChecks
{
    public static void FallbackRateAllocationStartsInRequested()
    {
        var item = FallbackRateAllocation.NewFallbackRateAllocation("code-411", "name-285");
        Check.Equal(item.Status, FallbackRateAllocationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackRateAllocationMovesToGranted()
    {
        var item = FallbackRateAllocation.NewFallbackRateAllocation("code-411", "name-285");
        try
        {
            item.MoveTo(FallbackRateAllocationStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackRateAllocationStatus.Granted, "status");
    }

    public static void FallbackRateAllocationRejectsMovesFromExpired()
    {
        var item = FallbackRateAllocation.NewFallbackRateAllocation("code-411", "name-285");
        item.Status = FallbackRateAllocationStatus.Expired;
        Check.Fails(() => item.MoveTo(FallbackRateAllocationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        FallbackRateAllocationStartsInRequested();
        FallbackRateAllocationMovesToGranted();
        FallbackRateAllocationRejectsMovesFromExpired();
    }
}
