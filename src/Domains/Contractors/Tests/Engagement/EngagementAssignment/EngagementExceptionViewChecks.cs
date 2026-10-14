namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementAssignment;

using Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>Checks of engagement assignment code, run by the project's runner.</summary>
public static class EngagementExceptionViewChecks
{
    public static void EngagementExceptionViewShowsTheStatusInCapitals()
    {
        var item = EngagementException.NewEngagementException("code-305", "title-668");
        var view = EngagementExceptionView.ToEngagementExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementExceptionView.ToEngagementExceptionViews(new List<EngagementException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementExceptionViewShowsTheStatusInCapitals();
    }
}
