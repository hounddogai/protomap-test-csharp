namespace Acme.Workforce.Assets.Tests.Assignment.Baseline;

using Acme.Workforce.Assets.Assignment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LegacyAssignmentBaselineChecks
{
    public static void LegacyAssignmentBaselineStartsInProposed()
    {
        var item = LegacyAssignmentBaseline.NewLegacyAssignmentBaseline("code-987", "title-392");
        Check.Equal(item.Status, LegacyAssignmentBaselineStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyAssignmentBaselineMovesToWithdrawn()
    {
        var item = LegacyAssignmentBaseline.NewLegacyAssignmentBaseline("code-987", "title-392");
        try
        {
            item.MoveTo(LegacyAssignmentBaselineStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyAssignmentBaselineStatus.Withdrawn, "status");
    }

    public static void LegacyAssignmentBaselineRejectsMovesFromWithdrawn()
    {
        var item = LegacyAssignmentBaseline.NewLegacyAssignmentBaseline("code-987", "title-392");
        item.Status = LegacyAssignmentBaselineStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacyAssignmentBaselineStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacyAssignmentBaselineStartsInProposed();
        LegacyAssignmentBaselineMovesToWithdrawn();
        LegacyAssignmentBaselineRejectsMovesFromWithdrawn();
    }
}
