namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class StrictLicenseSeatWindowChecks
{
    public static void StrictLicenseSeatWindowStartsInRequested()
    {
        var item = StrictLicenseSeatWindow.NewStrictLicenseSeatWindow("code-751", "label-536");
        Check.Equal(item.Status, StrictLicenseSeatWindowStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictLicenseSeatWindowMovesToRevoked()
    {
        var item = StrictLicenseSeatWindow.NewStrictLicenseSeatWindow("code-751", "label-536");
        try
        {
            item.MoveTo(StrictLicenseSeatWindowStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictLicenseSeatWindowStatus.Revoked, "status");
    }

    public static void StrictLicenseSeatWindowRejectsMovesFromExpired()
    {
        var item = StrictLicenseSeatWindow.NewStrictLicenseSeatWindow("code-751", "label-536");
        item.Status = StrictLicenseSeatWindowStatus.Expired;
        Check.Fails(() => item.MoveTo(StrictLicenseSeatWindowStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StrictLicenseSeatWindowStartsInRequested();
        StrictLicenseSeatWindowMovesToRevoked();
        StrictLicenseSeatWindowRejectsMovesFromExpired();
    }
}
