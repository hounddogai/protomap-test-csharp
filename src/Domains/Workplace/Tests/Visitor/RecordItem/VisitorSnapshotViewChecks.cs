namespace Acme.Workforce.Workplace.Tests.Visitor.RecordItem;

using Acme.Workforce.Workplace.Visitor.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class VisitorSnapshotViewChecks
{
    public static void VisitorSnapshotViewShowsTheStatusInCapitals()
    {
        var item = VisitorSnapshot.NewVisitorSnapshot("code-520", "label-627");
        var view = VisitorSnapshotView.ToVisitorSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorSnapshotView.ToVisitorSnapshotViews(new List<VisitorSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorSnapshotViewShowsTheStatusInCapitals();
    }
}
