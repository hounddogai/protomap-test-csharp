namespace Acme.Workforce.Assets.Tests.Assignment.Rollup;

using Acme.Workforce.Assets.Assignment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AssignmentDraftViewChecks
{
    public static void AssignmentDraftViewShowsTheStatusInCapitals()
    {
        var item = AssignmentDraft.NewAssignmentDraft("code-225", "label-696");
        var view = AssignmentDraftView.ToAssignmentDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssignmentDraftView.ToAssignmentDraftViews(new List<AssignmentDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssignmentDraftViewShowsTheStatusInCapitals();
    }
}
