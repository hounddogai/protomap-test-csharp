namespace Acme.Workforce.Assets.Tests.LicenseSeat.Summary;

using Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class LicenseSeatPreferenceChecks
{
    public static void LicenseSeatPreferenceStartsInPlanned()
    {
        var item = LicenseSeatPreference.NewLicenseSeatPreference("code-938", "title-949");
        Check.Equal(item.Status, LicenseSeatPreferenceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatPreferenceMovesToCancelled()
    {
        var item = LicenseSeatPreference.NewLicenseSeatPreference("code-938", "title-949");
        try
        {
            item.MoveTo(LicenseSeatPreferenceStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatPreferenceStatus.Cancelled, "status");
    }

    public static void LicenseSeatPreferenceRejectsMovesFromCancelled()
    {
        var item = LicenseSeatPreference.NewLicenseSeatPreference("code-938", "title-949");
        item.Status = LicenseSeatPreferenceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LicenseSeatPreferenceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LicenseSeatPreferenceStartsInPlanned();
        LicenseSeatPreferenceMovesToCancelled();
        LicenseSeatPreferenceRejectsMovesFromCancelled();
    }
}
