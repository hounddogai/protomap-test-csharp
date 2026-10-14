namespace Acme.Workforce.Payrollops.Tests.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Checks of default garnishment allocation code, run by the project's runner.</summary>
public static class LegacyGarnishmentSlotChecks
{
    public static void LegacyGarnishmentSlotStartsInDraft()
    {
        var item = LegacyGarnishmentSlot.NewLegacyGarnishmentSlot("code-718", "label-569");
        Check.Equal(item.Status, LegacyGarnishmentSlotStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyGarnishmentSlotMovesToPending()
    {
        var item = LegacyGarnishmentSlot.NewLegacyGarnishmentSlot("code-718", "label-569");
        try
        {
            item.MoveTo(LegacyGarnishmentSlotStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyGarnishmentSlotStatus.Pending, "status");
    }

    public static void LegacyGarnishmentSlotRejectsMovesFromClosed()
    {
        var item = LegacyGarnishmentSlot.NewLegacyGarnishmentSlot("code-718", "label-569");
        item.Status = LegacyGarnishmentSlotStatus.Closed;
        Check.Fails(() => item.MoveTo(LegacyGarnishmentSlotStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LegacyGarnishmentSlotStartsInDraft();
        LegacyGarnishmentSlotMovesToPending();
        LegacyGarnishmentSlotRejectsMovesFromClosed();
    }
}
