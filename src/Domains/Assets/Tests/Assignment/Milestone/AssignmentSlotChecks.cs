namespace Acme.Workforce.Assets.Tests.Assignment.Milestone;

using Acme.Workforce.Assets.Assignment.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AssignmentSlotChecks
{
    public static void AssignmentSlotStartsInRequested()
    {
        var item = AssignmentSlot.NewAssignmentSlot("code-274", "label-908");
        Check.Equal(item.Status, AssignmentSlotStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentSlotMovesToRevoked()
    {
        var item = AssignmentSlot.NewAssignmentSlot("code-274", "label-908");
        try
        {
            item.MoveTo(AssignmentSlotStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentSlotStatus.Revoked, "status");
    }

    public static void AssignmentSlotRejectsMovesFromExpired()
    {
        var item = AssignmentSlot.NewAssignmentSlot("code-274", "label-908");
        item.Status = AssignmentSlotStatus.Expired;
        Check.Fails(() => item.MoveTo(AssignmentSlotStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AssignmentSlotStartsInRequested();
        AssignmentSlotMovesToRevoked();
        AssignmentSlotRejectsMovesFromExpired();
    }
}
