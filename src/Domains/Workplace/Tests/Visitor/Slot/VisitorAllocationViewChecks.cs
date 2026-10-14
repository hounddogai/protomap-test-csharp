namespace Acme.Workforce.Workplace.Tests.Visitor.Slot;

using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class VisitorAllocationViewChecks
{
    public static void VisitorAllocationViewShowsTheStatusInCapitals()
    {
        var item = VisitorAllocation.NewVisitorAllocation("code-514", "label-597");
        var view = VisitorAllocationView.ToVisitorAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorAllocationView.ToVisitorAllocationViews(new List<VisitorAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorAllocationViewShowsTheStatusInCapitals();
    }
}
