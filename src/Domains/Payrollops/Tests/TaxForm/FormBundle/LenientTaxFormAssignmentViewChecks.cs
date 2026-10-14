namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormBundle;

using Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>Checks of form bundle code, run by the project's runner.</summary>
public static class LenientTaxFormAssignmentViewChecks
{
    public static void LenientTaxFormAssignmentViewShowsTheStatusInCapitals()
    {
        var item = LenientTaxFormAssignment.NewLenientTaxFormAssignment("code-218", "title-116");
        var view = LenientTaxFormAssignmentView.ToLenientTaxFormAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientTaxFormAssignmentView.ToLenientTaxFormAssignmentViews(new List<LenientTaxFormAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientTaxFormAssignmentViewShowsTheStatusInCapitals();
    }
}
