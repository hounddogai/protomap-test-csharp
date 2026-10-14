namespace Acme.Workforce.Assets.Tests.Assignment.Milestone;

using Acme.Workforce.Assets.Assignment.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AssignmentSlotViewChecks
{
    public static void AssignmentSlotViewShowsTheStatusInCapitals()
    {
        var item = AssignmentSlot.NewAssignmentSlot("code-274", "label-908");
        var view = AssignmentSlotView.ToAssignmentSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssignmentSlotView.ToAssignmentSlotViews(new List<AssignmentSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssignmentSlotViewShowsTheStatusInCapitals();
    }
}
