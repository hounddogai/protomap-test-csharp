namespace Acme.Workforce.Payrollops.Tests.TaxForm.Score;

using Acme.Workforce.Payrollops.TaxForm.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class TaxFormBatchViewChecks
{
    public static void TaxFormBatchViewShowsTheStatusInCapitals()
    {
        var item = TaxFormBatch.NewTaxFormBatch("code-897", "label-465");
        var view = TaxFormBatchView.ToTaxFormBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormBatchView.ToTaxFormBatchViews(new List<TaxFormBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormBatchViewShowsTheStatusInCapitals();
    }
}
