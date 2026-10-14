namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SeasonalLicenseSeatPreferenceChecks
{
    public static void SeasonalLicenseSeatPreferenceStartsInProposed()
    {
        var item = SeasonalLicenseSeatPreference.NewSeasonalLicenseSeatPreference("code-435", "name-136");
        Check.Equal(item.Status, SeasonalLicenseSeatPreferenceStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalLicenseSeatPreferenceMovesToRejected()
    {
        var item = SeasonalLicenseSeatPreference.NewSeasonalLicenseSeatPreference("code-435", "name-136");
        try
        {
            item.MoveTo(SeasonalLicenseSeatPreferenceStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalLicenseSeatPreferenceStatus.Rejected, "status");
    }

    public static void SeasonalLicenseSeatPreferenceRejectsMovesFromWithdrawn()
    {
        var item = SeasonalLicenseSeatPreference.NewSeasonalLicenseSeatPreference("code-435", "name-136");
        item.Status = SeasonalLicenseSeatPreferenceStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SeasonalLicenseSeatPreferenceStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SeasonalLicenseSeatPreferenceStartsInProposed();
        SeasonalLicenseSeatPreferenceMovesToRejected();
        SeasonalLicenseSeatPreferenceRejectsMovesFromWithdrawn();
    }
}
