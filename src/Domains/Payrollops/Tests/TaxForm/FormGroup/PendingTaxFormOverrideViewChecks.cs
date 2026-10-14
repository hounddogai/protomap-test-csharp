namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class PendingTaxFormOverrideViewChecks
{
    public static void PendingTaxFormOverrideViewShowsTheStatusInCapitals()
    {
        var item = PendingTaxFormOverride.NewPendingTaxFormOverride("code-423", "title-560");
        var view = PendingTaxFormOverrideView.ToPendingTaxFormOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingTaxFormOverrideView.ToPendingTaxFormOverrideViews(new List<PendingTaxFormOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingTaxFormOverrideViewShowsTheStatusInCapitals();
    }
}
