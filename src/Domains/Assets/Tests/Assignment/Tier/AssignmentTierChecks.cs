namespace Acme.Workforce.Assets.Tests.Assignment.Tier;

using Acme.Workforce.Assets.Assignment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AssignmentTierChecks
{
    public static void AssignmentTierStartsInDraft()
    {
        var item = AssignmentTier.NewAssignmentTier("code-785", "label-324");
        Check.Equal(item.Status, AssignmentTierStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentTierMovesToActive()
    {
        var item = AssignmentTier.NewAssignmentTier("code-785", "label-324");
        try
        {
            item.MoveTo(AssignmentTierStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentTierStatus.Active, "status");
    }

    public static void AssignmentTierRejectsMovesFromClosed()
    {
        var item = AssignmentTier.NewAssignmentTier("code-785", "label-324");
        item.Status = AssignmentTierStatus.Closed;
        Check.Fails(() => item.MoveTo(AssignmentTierStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AssignmentTierStartsInDraft();
        AssignmentTierMovesToActive();
        AssignmentTierRejectsMovesFromClosed();
    }
}
