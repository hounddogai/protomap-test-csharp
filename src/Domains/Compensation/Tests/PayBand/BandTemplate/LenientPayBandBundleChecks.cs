namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class LenientPayBandBundleChecks
{
    public static void LenientPayBandBundleStartsInRequested()
    {
        var item = LenientPayBandBundle.NewLenientPayBandBundle("code-54", "label-164");
        Check.Equal(item.Status, LenientPayBandBundleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientPayBandBundleMovesToGranted()
    {
        var item = LenientPayBandBundle.NewLenientPayBandBundle("code-54", "label-164");
        try
        {
            item.MoveTo(LenientPayBandBundleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientPayBandBundleStatus.Granted, "status");
    }

    public static void LenientPayBandBundleRejectsMovesFromExpired()
    {
        var item = LenientPayBandBundle.NewLenientPayBandBundle("code-54", "label-164");
        item.Status = LenientPayBandBundleStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientPayBandBundleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientPayBandBundleStartsInRequested();
        LenientPayBandBundleMovesToGranted();
        LenientPayBandBundleRejectsMovesFromExpired();
    }
}
