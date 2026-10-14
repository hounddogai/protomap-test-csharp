namespace Acme.Workforce.Payrollops.Tests.Payslip.Snapshot;

using Acme.Workforce.Payrollops.Payslip.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalPayslipSlotChecks
{
    public static void SeasonalPayslipSlotStartsInOpen()
    {
        var item = SeasonalPayslipSlot.NewSeasonalPayslipSlot("code-926", "name-483");
        Check.Equal(item.Status, SeasonalPayslipSlotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalPayslipSlotMovesToResolved()
    {
        var item = SeasonalPayslipSlot.NewSeasonalPayslipSlot("code-926", "name-483");
        try
        {
            item.MoveTo(SeasonalPayslipSlotStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalPayslipSlotStatus.Resolved, "status");
    }

    public static void SeasonalPayslipSlotRejectsMovesFromArchived()
    {
        var item = SeasonalPayslipSlot.NewSeasonalPayslipSlot("code-926", "name-483");
        item.Status = SeasonalPayslipSlotStatus.Archived;
        Check.Fails(() => item.MoveTo(SeasonalPayslipSlotStatus.Open), "move");
    }

    public static void RunAll()
    {
        SeasonalPayslipSlotStartsInOpen();
        SeasonalPayslipSlotMovesToResolved();
        SeasonalPayslipSlotRejectsMovesFromArchived();
    }
}
