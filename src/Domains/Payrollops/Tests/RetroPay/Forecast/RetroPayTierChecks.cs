namespace Acme.Workforce.Payrollops.Tests.RetroPay.Forecast;

using Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RetroPayTierChecks
{
    public static void RetroPayTierStartsInRequested()
    {
        var item = RetroPayTier.NewRetroPayTier("code-252", "label-61");
        Check.Equal(item.Status, RetroPayTierStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayTierMovesToGranted()
    {
        var item = RetroPayTier.NewRetroPayTier("code-252", "label-61");
        try
        {
            item.MoveTo(RetroPayTierStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayTierStatus.Granted, "status");
    }

    public static void RetroPayTierRejectsMovesFromExpired()
    {
        var item = RetroPayTier.NewRetroPayTier("code-252", "label-61");
        item.Status = RetroPayTierStatus.Expired;
        Check.Fails(() => item.MoveTo(RetroPayTierStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RetroPayTierStartsInRequested();
        RetroPayTierMovesToGranted();
        RetroPayTierRejectsMovesFromExpired();
    }
}
