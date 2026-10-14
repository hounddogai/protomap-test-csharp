namespace Acme.Workforce.Payrollops.Tests.TaxForm.Group;

using Acme.Workforce.Payrollops.TaxForm.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ScheduledTaxFormTemplateViewChecks
{
    public static void ScheduledTaxFormTemplateViewShowsTheStatusInCapitals()
    {
        var item = ScheduledTaxFormTemplate.NewScheduledTaxFormTemplate("code-19", "name-277");
        var view = ScheduledTaxFormTemplateView.ToScheduledTaxFormTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledTaxFormTemplateView.ToScheduledTaxFormTemplateViews(new List<ScheduledTaxFormTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledTaxFormTemplateViewShowsTheStatusInCapitals();
    }
}
