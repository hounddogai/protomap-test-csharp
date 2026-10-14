namespace Acme.Workforce.Talent.Tests.Mobility.Allocation;

using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RollingMobilityAllocationChecks
{
    public static void RollingMobilityAllocationStartsInCreated()
    {
        var item = RollingMobilityAllocation.NewRollingMobilityAllocation("code-72", "name-321");
        Check.Equal(item.Status, RollingMobilityAllocationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingMobilityAllocationMovesToValidated()
    {
        var item = RollingMobilityAllocation.NewRollingMobilityAllocation("code-72", "name-321");
        try
        {
            item.MoveTo(RollingMobilityAllocationStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingMobilityAllocationStatus.Validated, "status");
    }

    public static void RollingMobilityAllocationRejectsMovesFromRetired()
    {
        var item = RollingMobilityAllocation.NewRollingMobilityAllocation("code-72", "name-321");
        item.Status = RollingMobilityAllocationStatus.Retired;
        Check.Fails(() => item.MoveTo(RollingMobilityAllocationStatus.Created), "move");
    }

    public static void RunAll()
    {
        RollingMobilityAllocationStartsInCreated();
        RollingMobilityAllocationMovesToValidated();
        RollingMobilityAllocationRejectsMovesFromRetired();
    }
}
