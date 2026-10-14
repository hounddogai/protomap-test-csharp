namespace Acme.Workforce.Payrollops.Tests.Garnishment.Baseline;

using Acme.Workforce.Payrollops.Garnishment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackGarnishmentRecordViewChecks
{
    public static void FallbackGarnishmentRecordViewShowsTheStatusInCapitals()
    {
        var item = FallbackGarnishmentRecord.NewFallbackGarnishmentRecord("code-227", "title-819");
        var view = FallbackGarnishmentRecordView.ToFallbackGarnishmentRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackGarnishmentRecordView.ToFallbackGarnishmentRecordViews(new List<FallbackGarnishmentRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackGarnishmentRecordViewShowsTheStatusInCapitals();
    }
}
