namespace Acme.Workforce.Workplace.Tests.Visitor.Slot;

using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class VisitorRequestViewChecks
{
    public static void VisitorRequestViewShowsTheStatusInCapitals()
    {
        var item = VisitorRequest.NewVisitorRequest("code-705", "label-742");
        var view = VisitorRequestView.ToVisitorRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorRequestView.ToVisitorRequestViews(new List<VisitorRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorRequestViewShowsTheStatusInCapitals();
    }
}
