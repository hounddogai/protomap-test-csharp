namespace Acme.Workforce.Assets.Tests.Assignment.Batch;

using Acme.Workforce.Assets.Assignment.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PrimaryAssignmentTierChecks
{
    public static void PrimaryAssignmentTierStartsInOpen()
    {
        var item = PrimaryAssignmentTier.NewPrimaryAssignmentTier("code-75", "label-615");
        Check.Equal(item.Status, PrimaryAssignmentTierStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryAssignmentTierMovesToArchived()
    {
        var item = PrimaryAssignmentTier.NewPrimaryAssignmentTier("code-75", "label-615");
        try
        {
            item.MoveTo(PrimaryAssignmentTierStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryAssignmentTierStatus.Archived, "status");
    }

    public static void PrimaryAssignmentTierRejectsMovesFromArchived()
    {
        var item = PrimaryAssignmentTier.NewPrimaryAssignmentTier("code-75", "label-615");
        item.Status = PrimaryAssignmentTierStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryAssignmentTierStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryAssignmentTierStartsInOpen();
        PrimaryAssignmentTierMovesToArchived();
        PrimaryAssignmentTierRejectsMovesFromArchived();
    }
}
