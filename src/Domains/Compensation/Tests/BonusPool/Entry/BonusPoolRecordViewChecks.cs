namespace Acme.Workforce.Compensation.Tests.BonusPool.Entry;

using Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BonusPoolRecordViewChecks
{
    public static void BonusPoolRecordViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolRecord.NewBonusPoolRecord("code-279", "label-173");
        var view = BonusPoolRecordView.ToBonusPoolRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolRecordView.ToBonusPoolRecordViews(new List<BonusPoolRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolRecordViewShowsTheStatusInCapitals();
    }
}
