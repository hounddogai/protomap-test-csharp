namespace Acme.Workforce.Compensation.Tests.PayBand.Preference;

using Acme.Workforce.Compensation.PayBand.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PayBandPreferenceChecks
{
    public static void PayBandPreferenceStartsInNew()
    {
        var item = PayBandPreference.NewPayBandPreference("code-301", "title-777");
        Check.Equal(item.Status, PayBandPreferenceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandPreferenceMovesToQueued()
    {
        var item = PayBandPreference.NewPayBandPreference("code-301", "title-777");
        try
        {
            item.MoveTo(PayBandPreferenceStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandPreferenceStatus.Queued, "status");
    }

    public static void PayBandPreferenceRejectsMovesFromFailed()
    {
        var item = PayBandPreference.NewPayBandPreference("code-301", "title-777");
        item.Status = PayBandPreferenceStatus.Failed;
        Check.Fails(() => item.MoveTo(PayBandPreferenceStatus.New), "move");
    }

    public static void RunAll()
    {
        PayBandPreferenceStartsInNew();
        PayBandPreferenceMovesToQueued();
        PayBandPreferenceRejectsMovesFromFailed();
    }
}
