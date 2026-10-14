namespace Acme.Workforce.Payrollops.Tests.Deduction.Quota;

using Acme.Workforce.Payrollops.Deduction.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class DeductionQuotaViewChecks
{
    public static void DeductionQuotaViewShowsTheStatusInCapitals()
    {
        var item = DeductionQuota.NewDeductionQuota("code-12", "name-925");
        var view = DeductionQuotaView.ToDeductionQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeductionQuotaView.ToDeductionQuotaViews(new List<DeductionQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeductionQuotaViewShowsTheStatusInCapitals();
    }
}
