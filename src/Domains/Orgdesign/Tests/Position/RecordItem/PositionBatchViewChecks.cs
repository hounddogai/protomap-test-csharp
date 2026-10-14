namespace Acme.Workforce.Orgdesign.Tests.Position.RecordItem;

using Acme.Workforce.Orgdesign.Position.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PositionBatchViewChecks
{
    public static void PositionBatchViewShowsTheStatusInCapitals()
    {
        var item = PositionBatch.NewPositionBatch("code-876", "title-324");
        var view = PositionBatchView.ToPositionBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PositionBatchView.ToPositionBatchViews(new List<PositionBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PositionBatchViewShowsTheStatusInCapitals();
    }
}
