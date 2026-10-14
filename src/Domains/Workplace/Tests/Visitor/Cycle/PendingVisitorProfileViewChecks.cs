namespace Acme.Workforce.Workplace.Tests.Visitor.Cycle;

using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PendingVisitorProfileViewChecks
{
    public static void PendingVisitorProfileViewShowsTheStatusInCapitals()
    {
        var item = PendingVisitorProfile.NewPendingVisitorProfile("code-416", "title-123");
        var view = PendingVisitorProfileView.ToPendingVisitorProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingVisitorProfileView.ToPendingVisitorProfileViews(new List<PendingVisitorProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingVisitorProfileViewShowsTheStatusInCapitals();
    }
}
