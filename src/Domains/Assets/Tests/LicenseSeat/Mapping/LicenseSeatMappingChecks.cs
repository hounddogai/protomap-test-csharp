namespace Acme.Workforce.Assets.Tests.LicenseSeat.Mapping;

using Acme.Workforce.Assets.LicenseSeat.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LicenseSeatMappingChecks
{
    public static void LicenseSeatMappingStartsInOpen()
    {
        var item = LicenseSeatMapping.NewLicenseSeatMapping("code-479", "title-722");
        Check.Equal(item.Status, LicenseSeatMappingStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatMappingMovesToReopened()
    {
        var item = LicenseSeatMapping.NewLicenseSeatMapping("code-479", "title-722");
        try
        {
            item.MoveTo(LicenseSeatMappingStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatMappingStatus.Reopened, "status");
    }

    public static void LicenseSeatMappingRejectsMovesFromArchived()
    {
        var item = LicenseSeatMapping.NewLicenseSeatMapping("code-479", "title-722");
        item.Status = LicenseSeatMappingStatus.Archived;
        Check.Fails(() => item.MoveTo(LicenseSeatMappingStatus.Open), "move");
    }

    public static void RunAll()
    {
        LicenseSeatMappingStartsInOpen();
        LicenseSeatMappingMovesToReopened();
        LicenseSeatMappingRejectsMovesFromArchived();
    }
}
