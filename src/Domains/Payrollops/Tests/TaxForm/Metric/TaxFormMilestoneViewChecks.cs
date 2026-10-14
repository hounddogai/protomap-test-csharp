namespace Acme.Workforce.Payrollops.Tests.TaxForm.Metric;

using Acme.Workforce.Payrollops.TaxForm.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TaxFormMilestoneViewChecks
{
    public static void TaxFormMilestoneViewShowsTheStatusInCapitals()
    {
        var item = TaxFormMilestone.NewTaxFormMilestone("code-249", "title-929");
        var view = TaxFormMilestoneView.ToTaxFormMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormMilestoneView.ToTaxFormMilestoneViews(new List<TaxFormMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormMilestoneViewShowsTheStatusInCapitals();
    }
}
