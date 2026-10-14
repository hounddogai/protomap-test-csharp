namespace Acme.Workforce.Absence.Tests.Accrual.Checkpoint;

using Acme.Workforce.Absence.Accrual.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class ManualAccrualCheckpointViewChecks
{
    public static void ManualAccrualCheckpointViewShowsTheStatusInCapitals()
    {
        var item = ManualAccrualCheckpoint.NewManualAccrualCheckpoint("code-11", "title-960");
        var view = ManualAccrualCheckpointView.ToManualAccrualCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualAccrualCheckpointView.ToManualAccrualCheckpointViews(new List<ManualAccrualCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualAccrualCheckpointViewShowsTheStatusInCapitals();
    }
}
