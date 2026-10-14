namespace Acme.Workforce.Assets.Tests.Laptop.Target;

using Acme.Workforce.Assets.Laptop.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PendingLaptopTargetChecks
{
    public static void PendingLaptopTargetStartsInDraft()
    {
        var item = PendingLaptopTarget.NewPendingLaptopTarget("code-118", "label-24");
        Check.Equal(item.Status, PendingLaptopTargetStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingLaptopTargetMovesToPending()
    {
        var item = PendingLaptopTarget.NewPendingLaptopTarget("code-118", "label-24");
        try
        {
            item.MoveTo(PendingLaptopTargetStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingLaptopTargetStatus.Pending, "status");
    }

    public static void PendingLaptopTargetRejectsMovesFromClosed()
    {
        var item = PendingLaptopTarget.NewPendingLaptopTarget("code-118", "label-24");
        item.Status = PendingLaptopTargetStatus.Closed;
        Check.Fails(() => item.MoveTo(PendingLaptopTargetStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PendingLaptopTargetStartsInDraft();
        PendingLaptopTargetMovesToPending();
        PendingLaptopTargetRejectsMovesFromClosed();
    }
}
