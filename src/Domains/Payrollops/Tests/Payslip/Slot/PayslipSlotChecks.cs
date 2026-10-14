namespace Acme.Workforce.Payrollops.Tests.Payslip.Slot;

using Acme.Workforce.Payrollops.Payslip.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PayslipSlotChecks
{
    public static void PayslipSlotStartsInRequested()
    {
        var item = PayslipSlot.NewPayslipSlot("code-750", "title-118");
        Check.Equal(item.Status, PayslipSlotStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayslipSlotMovesToExpired()
    {
        var item = PayslipSlot.NewPayslipSlot("code-750", "title-118");
        try
        {
            item.MoveTo(PayslipSlotStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayslipSlotStatus.Expired, "status");
    }

    public static void PayslipSlotRejectsMovesFromExpired()
    {
        var item = PayslipSlot.NewPayslipSlot("code-750", "title-118");
        item.Status = PayslipSlotStatus.Expired;
        Check.Fails(() => item.MoveTo(PayslipSlotStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PayslipSlotStartsInRequested();
        PayslipSlotMovesToExpired();
        PayslipSlotRejectsMovesFromExpired();
    }
}
