namespace Acme.Workforce.Assets.Tests.Assignment.Request;

using Acme.Workforce.Assets.Assignment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AssignmentRequestViewChecks
{
    public static void AssignmentRequestViewShowsTheStatusInCapitals()
    {
        var item = AssignmentRequest.NewAssignmentRequest("code-462", "label-688");
        var view = AssignmentRequestView.ToAssignmentRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssignmentRequestView.ToAssignmentRequestViews(new List<AssignmentRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssignmentRequestViewShowsTheStatusInCapitals();
    }
}
