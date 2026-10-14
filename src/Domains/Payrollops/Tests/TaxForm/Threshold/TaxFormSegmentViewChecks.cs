namespace Acme.Workforce.Payrollops.Tests.TaxForm.Threshold;

using Acme.Workforce.Payrollops.TaxForm.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class TaxFormSegmentViewChecks
{
    public static void TaxFormSegmentViewShowsTheStatusInCapitals()
    {
        var item = TaxFormSegment.NewTaxFormSegment("code-115", "name-144");
        var view = TaxFormSegmentView.ToTaxFormSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormSegmentView.ToTaxFormSegmentViews(new List<TaxFormSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormSegmentViewShowsTheStatusInCapitals();
    }
}
