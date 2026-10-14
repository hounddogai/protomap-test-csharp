namespace Acme.Workforce.Engagement.Tests.Feedback.Manifest;

using Acme.Workforce.Engagement.Feedback.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PendingFeedbackMappingChecks
{
    public static void PendingFeedbackMappingStartsInProposed()
    {
        var item = PendingFeedbackMapping.NewPendingFeedbackMapping("code-571", "name-791");
        Check.Equal(item.Status, PendingFeedbackMappingStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingFeedbackMappingMovesToWithdrawn()
    {
        var item = PendingFeedbackMapping.NewPendingFeedbackMapping("code-571", "name-791");
        try
        {
            item.MoveTo(PendingFeedbackMappingStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingFeedbackMappingStatus.Withdrawn, "status");
    }

    public static void PendingFeedbackMappingRejectsMovesFromWithdrawn()
    {
        var item = PendingFeedbackMapping.NewPendingFeedbackMapping("code-571", "name-791");
        item.Status = PendingFeedbackMappingStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PendingFeedbackMappingStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PendingFeedbackMappingStartsInProposed();
        PendingFeedbackMappingMovesToWithdrawn();
        PendingFeedbackMappingRejectsMovesFromWithdrawn();
    }
}
