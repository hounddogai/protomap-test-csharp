namespace Acme.Workforce.Payrollops.Tests.RetroPay.Assignment;

using Acme.Workforce.Payrollops.RetroPay.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LenientRetroPayPreferenceChecks
{
    public static void LenientRetroPayPreferenceStartsInIdle()
    {
        var item = LenientRetroPayPreference.NewLenientRetroPayPreference("code-758", "title-788");
        Check.Equal(item.Status, LenientRetroPayPreferenceStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientRetroPayPreferenceMovesToReady()
    {
        var item = LenientRetroPayPreference.NewLenientRetroPayPreference("code-758", "title-788");
        try
        {
            item.MoveTo(LenientRetroPayPreferenceStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientRetroPayPreferenceStatus.Ready, "status");
    }

    public static void LenientRetroPayPreferenceRejectsMovesFromStopped()
    {
        var item = LenientRetroPayPreference.NewLenientRetroPayPreference("code-758", "title-788");
        item.Status = LenientRetroPayPreferenceStatus.Stopped;
        Check.Fails(() => item.MoveTo(LenientRetroPayPreferenceStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LenientRetroPayPreferenceStartsInIdle();
        LenientRetroPayPreferenceMovesToReady();
        LenientRetroPayPreferenceRejectsMovesFromStopped();
    }
}
