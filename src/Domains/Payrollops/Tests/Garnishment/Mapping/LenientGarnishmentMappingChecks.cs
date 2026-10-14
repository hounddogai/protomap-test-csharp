namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LenientGarnishmentMappingChecks
{
    public static void LenientGarnishmentMappingStartsInDraft()
    {
        var item = LenientGarnishmentMapping.NewLenientGarnishmentMapping("code-831", "label-388");
        Check.Equal(item.Status, LenientGarnishmentMappingStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientGarnishmentMappingMovesToClosed()
    {
        var item = LenientGarnishmentMapping.NewLenientGarnishmentMapping("code-831", "label-388");
        try
        {
            item.MoveTo(LenientGarnishmentMappingStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientGarnishmentMappingStatus.Closed, "status");
    }

    public static void LenientGarnishmentMappingRejectsMovesFromClosed()
    {
        var item = LenientGarnishmentMapping.NewLenientGarnishmentMapping("code-831", "label-388");
        item.Status = LenientGarnishmentMappingStatus.Closed;
        Check.Fails(() => item.MoveTo(LenientGarnishmentMappingStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LenientGarnishmentMappingStartsInDraft();
        LenientGarnishmentMappingMovesToClosed();
        LenientGarnishmentMappingRejectsMovesFromClosed();
    }
}
