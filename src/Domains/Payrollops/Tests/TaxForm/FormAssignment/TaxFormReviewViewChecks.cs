namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormAssignment;

using Acme.Workforce.Payrollops.TaxForm.FormAssignment;

/// <summary>Checks of form assignment code, run by the project's runner.</summary>
public static class TaxFormReviewViewChecks
{
    public static void TaxFormReviewViewShowsTheStatusInCapitals()
    {
        var item = TaxFormReview.NewTaxFormReview("code-839", "label-497");
        var view = TaxFormReviewView.ToTaxFormReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormReviewView.ToTaxFormReviewViews(new List<TaxFormReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormReviewViewShowsTheStatusInCapitals();
    }
}
