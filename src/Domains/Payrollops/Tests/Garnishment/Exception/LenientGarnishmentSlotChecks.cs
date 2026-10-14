namespace Acme.Workforce.Payrollops.Tests.Garnishment.Exception;

using Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class LenientGarnishmentSlotChecks
{
    public static void LenientGarnishmentSlotStartsInPlanned()
    {
        var item = LenientGarnishmentSlot.NewLenientGarnishmentSlot("code-657", "title-494");
        Check.Equal(item.Status, LenientGarnishmentSlotStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientGarnishmentSlotMovesToCancelled()
    {
        var item = LenientGarnishmentSlot.NewLenientGarnishmentSlot("code-657", "title-494");
        try
        {
            item.MoveTo(LenientGarnishmentSlotStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientGarnishmentSlotStatus.Cancelled, "status");
    }

    public static void LenientGarnishmentSlotRejectsMovesFromCancelled()
    {
        var item = LenientGarnishmentSlot.NewLenientGarnishmentSlot("code-657", "title-494");
        item.Status = LenientGarnishmentSlotStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientGarnishmentSlotStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientGarnishmentSlotStartsInPlanned();
        LenientGarnishmentSlotMovesToCancelled();
        LenientGarnishmentSlotRejectsMovesFromCancelled();
    }
}
