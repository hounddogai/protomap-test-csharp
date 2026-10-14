namespace Acme.Workforce.Absence.Tests.Accrual.Cycle;

using Acme.Workforce.Absence.Accrual.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AccrualPreferenceChecks
{
    public static void AccrualPreferenceStartsInRequested()
    {
        var item = AccrualPreference.NewAccrualPreference("code-785", "title-197");
        Check.Equal(item.Status, AccrualPreferenceStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualPreferenceMovesToRevoked()
    {
        var item = AccrualPreference.NewAccrualPreference("code-785", "title-197");
        try
        {
            item.MoveTo(AccrualPreferenceStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualPreferenceStatus.Revoked, "status");
    }

    public static void AccrualPreferenceRejectsMovesFromExpired()
    {
        var item = AccrualPreference.NewAccrualPreference("code-785", "title-197");
        item.Status = AccrualPreferenceStatus.Expired;
        Check.Fails(() => item.MoveTo(AccrualPreferenceStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AccrualPreferenceStartsInRequested();
        AccrualPreferenceMovesToRevoked();
        AccrualPreferenceRejectsMovesFromExpired();
    }
}
