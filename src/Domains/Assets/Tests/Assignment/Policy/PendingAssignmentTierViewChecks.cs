namespace Acme.Workforce.Assets.Tests.Assignment.Policy;

using Acme.Workforce.Assets.Assignment.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PendingAssignmentTierViewChecks
{
    public static void PendingAssignmentTierViewShowsTheStatusInCapitals()
    {
        var item = PendingAssignmentTier.NewPendingAssignmentTier("code-307", "title-719");
        var view = PendingAssignmentTierView.ToPendingAssignmentTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingAssignmentTierView.ToPendingAssignmentTierViews(new List<PendingAssignmentTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingAssignmentTierViewShowsTheStatusInCapitals();
    }
}
