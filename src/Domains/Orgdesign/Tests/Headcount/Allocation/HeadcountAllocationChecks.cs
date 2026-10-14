namespace Acme.Workforce.Orgdesign.Tests.Headcount.Allocation;

using Acme.Workforce.Orgdesign.Headcount.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class HeadcountAllocationChecks
{
    public static void HeadcountAllocationStartsInRequested()
    {
        var item = HeadcountAllocation.NewHeadcountAllocation("code-921", "label-17");
        Check.Equal(item.Status, HeadcountAllocationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountAllocationMovesToRevoked()
    {
        var item = HeadcountAllocation.NewHeadcountAllocation("code-921", "label-17");
        try
        {
            item.MoveTo(HeadcountAllocationStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountAllocationStatus.Revoked, "status");
    }

    public static void HeadcountAllocationRejectsMovesFromExpired()
    {
        var item = HeadcountAllocation.NewHeadcountAllocation("code-921", "label-17");
        item.Status = HeadcountAllocationStatus.Expired;
        Check.Fails(() => item.MoveTo(HeadcountAllocationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        HeadcountAllocationStartsInRequested();
        HeadcountAllocationMovesToRevoked();
        HeadcountAllocationRejectsMovesFromExpired();
    }
}
