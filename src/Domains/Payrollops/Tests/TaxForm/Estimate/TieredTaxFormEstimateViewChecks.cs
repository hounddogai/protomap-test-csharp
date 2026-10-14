namespace Acme.Workforce.Payrollops.Tests.TaxForm.Estimate;

using Acme.Workforce.Payrollops.TaxForm.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TieredTaxFormEstimateViewChecks
{
    public static void TieredTaxFormEstimateViewShowsTheStatusInCapitals()
    {
        var item = TieredTaxFormEstimate.NewTieredTaxFormEstimate("code-415", "name-118");
        var view = TieredTaxFormEstimateView.ToTieredTaxFormEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredTaxFormEstimateView.ToTieredTaxFormEstimateViews(new List<TieredTaxFormEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredTaxFormEstimateViewShowsTheStatusInCapitals();
    }
}
