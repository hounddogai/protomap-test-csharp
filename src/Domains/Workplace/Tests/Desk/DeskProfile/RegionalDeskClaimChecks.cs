namespace Acme.Workforce.Workplace.Tests.Desk.DeskProfile;

using Acme.Workforce.Workplace.Desk.DeskProfile;

/// <summary>Checks of desk profile code, run by the project's runner.</summary>
public static class RegionalDeskClaimChecks
{
    public static void RegionalDeskClaimStartsInCreated()
    {
        var item = RegionalDeskClaim.NewRegionalDeskClaim("code-714", "name-85");
        Check.Equal(item.Status, RegionalDeskClaimStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalDeskClaimMovesToValidated()
    {
        var item = RegionalDeskClaim.NewRegionalDeskClaim("code-714", "name-85");
        try
        {
            item.MoveTo(RegionalDeskClaimStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalDeskClaimStatus.Validated, "status");
    }

    public static void RegionalDeskClaimRejectsMovesFromRetired()
    {
        var item = RegionalDeskClaim.NewRegionalDeskClaim("code-714", "name-85");
        item.Status = RegionalDeskClaimStatus.Retired;
        Check.Fails(() => item.MoveTo(RegionalDeskClaimStatus.Created), "move");
    }

    public static void RunAll()
    {
        RegionalDeskClaimStartsInCreated();
        RegionalDeskClaimMovesToValidated();
        RegionalDeskClaimRejectsMovesFromRetired();
    }
}
