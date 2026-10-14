namespace Acme.Workforce.Orgdesign.Tests.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

/// <summary>Checks of legacy headcount record code, run by the project's runner.</summary>
public static class AdaptiveHeadcountRecordViewChecks
{
    public static void AdaptiveHeadcountRecordViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveHeadcountRecord.NewAdaptiveHeadcountRecord("code-830", "label-533");
        var view = AdaptiveHeadcountRecordView.ToAdaptiveHeadcountRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveHeadcountRecordView.ToAdaptiveHeadcountRecordViews(new List<AdaptiveHeadcountRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveHeadcountRecordViewShowsTheStatusInCapitals();
    }
}
