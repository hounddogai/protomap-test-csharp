namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class VisitorClaimViewChecks
{
    public static void VisitorClaimViewShowsTheStatusInCapitals()
    {
        var item = VisitorClaim.NewVisitorClaim("code-857", "label-600");
        var view = VisitorClaimView.ToVisitorClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorClaimView.ToVisitorClaimViews(new List<VisitorClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorClaimViewShowsTheStatusInCapitals();
    }
}
