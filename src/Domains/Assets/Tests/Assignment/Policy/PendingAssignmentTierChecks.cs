namespace Acme.Workforce.Assets.Tests.Assignment.Policy;

using Acme.Workforce.Assets.Assignment.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PendingAssignmentTierChecks
{
    public static void PendingAssignmentTierStartsInDraft()
    {
        var item = PendingAssignmentTier.NewPendingAssignmentTier("code-307", "title-719");
        Check.Equal(item.Status, PendingAssignmentTierStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingAssignmentTierMovesToActive()
    {
        var item = PendingAssignmentTier.NewPendingAssignmentTier("code-307", "title-719");
        try
        {
            item.MoveTo(PendingAssignmentTierStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingAssignmentTierStatus.Active, "status");
    }

    public static void PendingAssignmentTierRejectsMovesFromClosed()
    {
        var item = PendingAssignmentTier.NewPendingAssignmentTier("code-307", "title-719");
        item.Status = PendingAssignmentTierStatus.Closed;
        Check.Fails(() => item.MoveTo(PendingAssignmentTierStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PendingAssignmentTierStartsInDraft();
        PendingAssignmentTierMovesToActive();
        PendingAssignmentTierRejectsMovesFromClosed();
    }
}
