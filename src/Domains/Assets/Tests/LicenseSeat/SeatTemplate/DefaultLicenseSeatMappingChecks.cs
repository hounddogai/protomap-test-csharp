namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatTemplate;

using Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>Checks of seat template code, run by the project's runner.</summary>
public static class DefaultLicenseSeatMappingChecks
{
    public static void DefaultLicenseSeatMappingStartsInRequested()
    {
        var item = DefaultLicenseSeatMapping.NewDefaultLicenseSeatMapping("code-911", "title-494");
        Check.Equal(item.Status, DefaultLicenseSeatMappingStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultLicenseSeatMappingMovesToExpired()
    {
        var item = DefaultLicenseSeatMapping.NewDefaultLicenseSeatMapping("code-911", "title-494");
        try
        {
            item.MoveTo(DefaultLicenseSeatMappingStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultLicenseSeatMappingStatus.Expired, "status");
    }

    public static void DefaultLicenseSeatMappingRejectsMovesFromExpired()
    {
        var item = DefaultLicenseSeatMapping.NewDefaultLicenseSeatMapping("code-911", "title-494");
        item.Status = DefaultLicenseSeatMappingStatus.Expired;
        Check.Fails(() => item.MoveTo(DefaultLicenseSeatMappingStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DefaultLicenseSeatMappingStartsInRequested();
        DefaultLicenseSeatMappingMovesToExpired();
        DefaultLicenseSeatMappingRejectsMovesFromExpired();
    }
}
