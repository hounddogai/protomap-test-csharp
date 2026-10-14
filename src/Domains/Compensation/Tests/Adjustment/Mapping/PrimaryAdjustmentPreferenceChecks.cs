namespace Acme.Workforce.Compensation.Tests.Adjustment.Mapping;

using Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PrimaryAdjustmentPreferenceChecks
{
    public static void PrimaryAdjustmentPreferenceStartsInRequested()
    {
        var item = PrimaryAdjustmentPreference.NewPrimaryAdjustmentPreference("code-995", "label-564");
        Check.Equal(item.Status, PrimaryAdjustmentPreferenceStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryAdjustmentPreferenceMovesToGranted()
    {
        var item = PrimaryAdjustmentPreference.NewPrimaryAdjustmentPreference("code-995", "label-564");
        try
        {
            item.MoveTo(PrimaryAdjustmentPreferenceStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryAdjustmentPreferenceStatus.Granted, "status");
    }

    public static void PrimaryAdjustmentPreferenceRejectsMovesFromExpired()
    {
        var item = PrimaryAdjustmentPreference.NewPrimaryAdjustmentPreference("code-995", "label-564");
        item.Status = PrimaryAdjustmentPreferenceStatus.Expired;
        Check.Fails(() => item.MoveTo(PrimaryAdjustmentPreferenceStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PrimaryAdjustmentPreferenceStartsInRequested();
        PrimaryAdjustmentPreferenceMovesToGranted();
        PrimaryAdjustmentPreferenceRejectsMovesFromExpired();
    }
}
