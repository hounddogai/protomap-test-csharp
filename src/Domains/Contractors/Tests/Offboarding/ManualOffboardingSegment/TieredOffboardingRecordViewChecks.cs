namespace Acme.Workforce.Contractors.Tests.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

/// <summary>Checks of manual offboarding segment code, run by the project's runner.</summary>
public static class TieredOffboardingRecordViewChecks
{
    public static void TieredOffboardingRecordViewShowsTheStatusInCapitals()
    {
        var item = TieredOffboardingRecord.NewTieredOffboardingRecord("code-33", "name-610");
        var view = TieredOffboardingRecordView.ToTieredOffboardingRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredOffboardingRecordView.ToTieredOffboardingRecordViews(new List<TieredOffboardingRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredOffboardingRecordViewShowsTheStatusInCapitals();
    }
}
