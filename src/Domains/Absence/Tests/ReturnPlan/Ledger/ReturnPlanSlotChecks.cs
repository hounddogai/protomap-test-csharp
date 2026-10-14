namespace Acme.Workforce.Absence.Tests.ReturnPlan.Ledger;

using Acme.Workforce.Absence.ReturnPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ReturnPlanSlotChecks
{
    public static void ReturnPlanSlotStartsInCreated()
    {
        var item = ReturnPlanSlot.NewReturnPlanSlot("code-646", "label-425");
        Check.Equal(item.Status, ReturnPlanSlotStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanSlotMovesToRetired()
    {
        var item = ReturnPlanSlot.NewReturnPlanSlot("code-646", "label-425");
        try
        {
            item.MoveTo(ReturnPlanSlotStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanSlotStatus.Retired, "status");
    }

    public static void ReturnPlanSlotRejectsMovesFromRetired()
    {
        var item = ReturnPlanSlot.NewReturnPlanSlot("code-646", "label-425");
        item.Status = ReturnPlanSlotStatus.Retired;
        Check.Fails(() => item.MoveTo(ReturnPlanSlotStatus.Created), "move");
    }

    public static void RunAll()
    {
        ReturnPlanSlotStartsInCreated();
        ReturnPlanSlotMovesToRetired();
        ReturnPlanSlotRejectsMovesFromRetired();
    }
}
