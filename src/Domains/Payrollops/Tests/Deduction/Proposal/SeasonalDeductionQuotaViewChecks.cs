namespace Acme.Workforce.Payrollops.Tests.Deduction.Proposal;

using Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SeasonalDeductionQuotaViewChecks
{
    public static void SeasonalDeductionQuotaViewShowsTheStatusInCapitals()
    {
        var item = SeasonalDeductionQuota.NewSeasonalDeductionQuota("code-242", "label-335");
        var view = SeasonalDeductionQuotaView.ToSeasonalDeductionQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalDeductionQuotaView.ToSeasonalDeductionQuotaViews(new List<SeasonalDeductionQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalDeductionQuotaViewShowsTheStatusInCapitals();
    }
}
