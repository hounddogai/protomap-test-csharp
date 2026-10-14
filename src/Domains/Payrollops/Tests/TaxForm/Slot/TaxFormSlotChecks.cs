namespace Acme.Workforce.Payrollops.Tests.TaxForm.Slot;

using Acme.Workforce.Payrollops.TaxForm.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class TaxFormSlotChecks
{
    public static void TaxFormSlotStartsInIdle()
    {
        var item = TaxFormSlot.NewTaxFormSlot("code-546", "title-167");
        Check.Equal(item.Status, TaxFormSlotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormSlotMovesToDraining()
    {
        var item = TaxFormSlot.NewTaxFormSlot("code-546", "title-167");
        try
        {
            item.MoveTo(TaxFormSlotStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormSlotStatus.Draining, "status");
    }

    public static void TaxFormSlotRejectsMovesFromStopped()
    {
        var item = TaxFormSlot.NewTaxFormSlot("code-546", "title-167");
        item.Status = TaxFormSlotStatus.Stopped;
        Check.Fails(() => item.MoveTo(TaxFormSlotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TaxFormSlotStartsInIdle();
        TaxFormSlotMovesToDraining();
        TaxFormSlotRejectsMovesFromStopped();
    }
}
