namespace Acme.Workforce.Payrollops.Tests.TaxForm.Assignment;

using Acme.Workforce.Payrollops.TaxForm.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TaxFormAssignmentViewChecks
{
    public static void TaxFormAssignmentViewShowsTheStatusInCapitals()
    {
        var item = TaxFormAssignment.NewTaxFormAssignment("code-111", "title-385");
        var view = TaxFormAssignmentView.ToTaxFormAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormAssignmentView.ToTaxFormAssignmentViews(new List<TaxFormAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormAssignmentViewShowsTheStatusInCapitals();
    }
}
