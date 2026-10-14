namespace Acme.Workforce.Compliance.Tests.Exception.Checkpoint;

using Acme.Workforce.Compliance.Exception.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class GlobalExceptionSnapshotViewChecks
{
    public static void GlobalExceptionSnapshotViewShowsTheStatusInCapitals()
    {
        var item = GlobalExceptionSnapshot.NewGlobalExceptionSnapshot("code-513", "title-154");
        var view = GlobalExceptionSnapshotView.ToGlobalExceptionSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalExceptionSnapshotView.ToGlobalExceptionSnapshotViews(new List<GlobalExceptionSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalExceptionSnapshotViewShowsTheStatusInCapitals();
    }
}
