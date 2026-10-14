namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentPreferenceChecks
{
    public static void AdjustmentPreferenceStartsInProposed()
    {
        var item = AdjustmentPreference.NewAdjustmentPreference("code-452", "label-944");
        Check.Equal(item.Status, AdjustmentPreferenceStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentPreferenceMovesToRejected()
    {
        var item = AdjustmentPreference.NewAdjustmentPreference("code-452", "label-944");
        try
        {
            item.MoveTo(AdjustmentPreferenceStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentPreferenceStatus.Rejected, "status");
    }

    public static void AdjustmentPreferenceRejectsMovesFromWithdrawn()
    {
        var item = AdjustmentPreference.NewAdjustmentPreference("code-452", "label-944");
        item.Status = AdjustmentPreferenceStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AdjustmentPreferenceStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AdjustmentPreferenceStartsInProposed();
        AdjustmentPreferenceMovesToRejected();
        AdjustmentPreferenceRejectsMovesFromWithdrawn();
    }
}
