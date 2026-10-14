namespace Acme.Workforce.Workplace.Tests.Visitor.Estimate;

using Acme.Workforce.Workplace.Visitor.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PendingVisitorPolicyViewChecks
{
    public static void PendingVisitorPolicyViewShowsTheStatusInCapitals()
    {
        var item = PendingVisitorPolicy.NewPendingVisitorPolicy("code-710", "label-736");
        var view = PendingVisitorPolicyView.ToPendingVisitorPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingVisitorPolicyView.ToPendingVisitorPolicyViews(new List<PendingVisitorPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingVisitorPolicyViewShowsTheStatusInCapitals();
    }
}
