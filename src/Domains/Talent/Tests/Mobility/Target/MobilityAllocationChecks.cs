namespace Acme.Workforce.Talent.Tests.Mobility.Target;

using Acme.Workforce.Talent.Mobility.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MobilityAllocationChecks
{
    public static void MobilityAllocationStartsInNew()
    {
        var item = MobilityAllocation.NewMobilityAllocation("code-482", "name-887");
        Check.Equal(item.Status, MobilityAllocationStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityAllocationMovesToRunning()
    {
        var item = MobilityAllocation.NewMobilityAllocation("code-482", "name-887");
        try
        {
            item.MoveTo(MobilityAllocationStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityAllocationStatus.Running, "status");
    }

    public static void MobilityAllocationRejectsMovesFromFailed()
    {
        var item = MobilityAllocation.NewMobilityAllocation("code-482", "name-887");
        item.Status = MobilityAllocationStatus.Failed;
        Check.Fails(() => item.MoveTo(MobilityAllocationStatus.New), "move");
    }

    public static void RunAll()
    {
        MobilityAllocationStartsInNew();
        MobilityAllocationMovesToRunning();
        MobilityAllocationRejectsMovesFromFailed();
    }
}
