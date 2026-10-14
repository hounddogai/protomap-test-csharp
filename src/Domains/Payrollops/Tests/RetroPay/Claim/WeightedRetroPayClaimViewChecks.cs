namespace Acme.Workforce.Payrollops.Tests.RetroPay.Claim;

using Acme.Workforce.Payrollops.RetroPay.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class WeightedRetroPayClaimViewChecks
{
    public static void WeightedRetroPayClaimViewShowsTheStatusInCapitals()
    {
        var item = WeightedRetroPayClaim.NewWeightedRetroPayClaim("code-796", "title-693");
        var view = WeightedRetroPayClaimView.ToWeightedRetroPayClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedRetroPayClaimView.ToWeightedRetroPayClaimViews(new List<WeightedRetroPayClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedRetroPayClaimViewShowsTheStatusInCapitals();
    }
}
