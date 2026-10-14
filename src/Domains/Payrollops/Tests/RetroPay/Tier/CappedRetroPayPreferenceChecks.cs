namespace Acme.Workforce.Payrollops.Tests.RetroPay.Tier;

using Acme.Workforce.Payrollops.RetroPay.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class CappedRetroPayPreferenceChecks
{
    public static void CappedRetroPayPreferenceStartsInCreated()
    {
        var item = CappedRetroPayPreference.NewCappedRetroPayPreference("code-32", "label-406");
        Check.Equal(item.Status, CappedRetroPayPreferenceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedRetroPayPreferenceMovesToValidated()
    {
        var item = CappedRetroPayPreference.NewCappedRetroPayPreference("code-32", "label-406");
        try
        {
            item.MoveTo(CappedRetroPayPreferenceStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedRetroPayPreferenceStatus.Validated, "status");
    }

    public static void CappedRetroPayPreferenceRejectsMovesFromRetired()
    {
        var item = CappedRetroPayPreference.NewCappedRetroPayPreference("code-32", "label-406");
        item.Status = CappedRetroPayPreferenceStatus.Retired;
        Check.Fails(() => item.MoveTo(CappedRetroPayPreferenceStatus.Created), "move");
    }

    public static void RunAll()
    {
        CappedRetroPayPreferenceStartsInCreated();
        CappedRetroPayPreferenceMovesToValidated();
        CappedRetroPayPreferenceRejectsMovesFromRetired();
    }
}
