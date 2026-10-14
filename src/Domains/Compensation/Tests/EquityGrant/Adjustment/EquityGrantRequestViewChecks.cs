namespace Acme.Workforce.Compensation.Tests.EquityGrant.Adjustment;

using Acme.Workforce.Compensation.EquityGrant.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EquityGrantRequestViewChecks
{
    public static void EquityGrantRequestViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantRequest.NewEquityGrantRequest("code-477", "title-792");
        var view = EquityGrantRequestView.ToEquityGrantRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantRequestView.ToEquityGrantRequestViews(new List<EquityGrantRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantRequestViewShowsTheStatusInCapitals();
    }
}
