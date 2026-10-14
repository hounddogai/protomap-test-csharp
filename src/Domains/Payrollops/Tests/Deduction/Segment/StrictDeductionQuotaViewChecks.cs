namespace Acme.Workforce.Payrollops.Tests.Deduction.Segment;

using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StrictDeductionQuotaViewChecks
{
    public static void StrictDeductionQuotaViewShowsTheStatusInCapitals()
    {
        var item = StrictDeductionQuota.NewStrictDeductionQuota("code-365", "label-753");
        var view = StrictDeductionQuotaView.ToStrictDeductionQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictDeductionQuotaView.ToStrictDeductionQuotaViews(new List<StrictDeductionQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictDeductionQuotaViewShowsTheStatusInCapitals();
    }
}
