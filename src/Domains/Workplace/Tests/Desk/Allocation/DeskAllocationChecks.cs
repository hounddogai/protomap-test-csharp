namespace Acme.Workforce.Workplace.Tests.Desk.Allocation;

using Acme.Workforce.Workplace.Desk.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class DeskAllocationChecks
{
    public static void DeskAllocationStartsInCreated()
    {
        var item = DeskAllocation.NewDeskAllocation("code-763", "title-881");
        Check.Equal(item.Status, DeskAllocationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskAllocationMovesToPublished()
    {
        var item = DeskAllocation.NewDeskAllocation("code-763", "title-881");
        try
        {
            item.MoveTo(DeskAllocationStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskAllocationStatus.Published, "status");
    }

    public static void DeskAllocationRejectsMovesFromRetired()
    {
        var item = DeskAllocation.NewDeskAllocation("code-763", "title-881");
        item.Status = DeskAllocationStatus.Retired;
        Check.Fails(() => item.MoveTo(DeskAllocationStatus.Created), "move");
    }

    public static void RunAll()
    {
        DeskAllocationStartsInCreated();
        DeskAllocationMovesToPublished();
        DeskAllocationRejectsMovesFromRetired();
    }
}
