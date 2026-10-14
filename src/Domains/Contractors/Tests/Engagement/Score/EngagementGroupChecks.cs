namespace Acme.Workforce.Contractors.Tests.Engagement.Score;

using Acme.Workforce.Contractors.Engagement.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class EngagementGroupChecks
{
    public static void EngagementGroupStartsInNew()
    {
        var item = EngagementGroup.NewEngagementGroup("code-572", "label-646");
        Check.Equal(item.Status, EngagementGroupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementGroupMovesToQueued()
    {
        var item = EngagementGroup.NewEngagementGroup("code-572", "label-646");
        try
        {
            item.MoveTo(EngagementGroupStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementGroupStatus.Queued, "status");
    }

    public static void EngagementGroupRejectsMovesFromFailed()
    {
        var item = EngagementGroup.NewEngagementGroup("code-572", "label-646");
        item.Status = EngagementGroupStatus.Failed;
        Check.Fails(() => item.MoveTo(EngagementGroupStatus.New), "move");
    }

    public static void RunAll()
    {
        EngagementGroupStartsInNew();
        EngagementGroupMovesToQueued();
        EngagementGroupRejectsMovesFromFailed();
    }
}
