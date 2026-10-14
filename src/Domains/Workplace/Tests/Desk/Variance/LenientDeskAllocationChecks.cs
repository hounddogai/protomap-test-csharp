namespace Acme.Workforce.Workplace.Tests.Desk.Variance;

using Acme.Workforce.Workplace.Desk.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class LenientDeskAllocationChecks
{
    public static void LenientDeskAllocationStartsInRequested()
    {
        var item = LenientDeskAllocation.NewLenientDeskAllocation("code-237", "title-827");
        Check.Equal(item.Status, LenientDeskAllocationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientDeskAllocationMovesToGranted()
    {
        var item = LenientDeskAllocation.NewLenientDeskAllocation("code-237", "title-827");
        try
        {
            item.MoveTo(LenientDeskAllocationStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientDeskAllocationStatus.Granted, "status");
    }

    public static void LenientDeskAllocationRejectsMovesFromExpired()
    {
        var item = LenientDeskAllocation.NewLenientDeskAllocation("code-237", "title-827");
        item.Status = LenientDeskAllocationStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientDeskAllocationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientDeskAllocationStartsInRequested();
        LenientDeskAllocationMovesToGranted();
        LenientDeskAllocationRejectsMovesFromExpired();
    }
}
