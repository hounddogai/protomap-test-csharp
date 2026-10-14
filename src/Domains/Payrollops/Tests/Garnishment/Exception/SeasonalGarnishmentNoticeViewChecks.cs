namespace Acme.Workforce.Payrollops.Tests.Garnishment.Exception;

using Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class SeasonalGarnishmentNoticeViewChecks
{
    public static void SeasonalGarnishmentNoticeViewShowsTheStatusInCapitals()
    {
        var item = SeasonalGarnishmentNotice.NewSeasonalGarnishmentNotice("code-855", "label-764");
        var view = SeasonalGarnishmentNoticeView.ToSeasonalGarnishmentNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalGarnishmentNoticeView.ToSeasonalGarnishmentNoticeViews(new List<SeasonalGarnishmentNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalGarnishmentNoticeViewShowsTheStatusInCapitals();
    }
}
