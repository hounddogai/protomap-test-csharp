namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PrimaryGarnishmentMappingChecks
{
    public static void PrimaryGarnishmentMappingStartsInDraft()
    {
        var item = PrimaryGarnishmentMapping.NewPrimaryGarnishmentMapping("code-519", "name-206");
        Check.Equal(item.Status, PrimaryGarnishmentMappingStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryGarnishmentMappingMovesToSuspended()
    {
        var item = PrimaryGarnishmentMapping.NewPrimaryGarnishmentMapping("code-519", "name-206");
        try
        {
            item.MoveTo(PrimaryGarnishmentMappingStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryGarnishmentMappingStatus.Suspended, "status");
    }

    public static void PrimaryGarnishmentMappingRejectsMovesFromClosed()
    {
        var item = PrimaryGarnishmentMapping.NewPrimaryGarnishmentMapping("code-519", "name-206");
        item.Status = PrimaryGarnishmentMappingStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryGarnishmentMappingStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryGarnishmentMappingStartsInDraft();
        PrimaryGarnishmentMappingMovesToSuspended();
        PrimaryGarnishmentMappingRejectsMovesFromClosed();
    }
}
