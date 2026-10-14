namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class ReorgAssignmentViewChecks
{
    public static void ReorgAssignmentViewShowsTheStatusInCapitals()
    {
        var item = ReorgAssignment.NewReorgAssignment("code-814", "title-183");
        var view = ReorgAssignmentView.ToReorgAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgAssignmentView.ToReorgAssignmentViews(new List<ReorgAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgAssignmentViewShowsTheStatusInCapitals();
    }
}
