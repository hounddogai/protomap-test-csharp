namespace Acme.Workforce.Absence.Tests.Policy.Assignment;

using Acme.Workforce.Absence.Policy.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PolicyEntryChecks
{
    public static void PolicyEntryStartsInProposed()
    {
        var item = PolicyEntry.NewPolicyEntry("code-103", "name-270");
        Check.Equal(item.Status, PolicyEntryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyEntryMovesToApproved()
    {
        var item = PolicyEntry.NewPolicyEntry("code-103", "name-270");
        try
        {
            item.MoveTo(PolicyEntryStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyEntryStatus.Approved, "status");
    }

    public static void PolicyEntryRejectsMovesFromWithdrawn()
    {
        var item = PolicyEntry.NewPolicyEntry("code-103", "name-270");
        item.Status = PolicyEntryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PolicyEntryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PolicyEntryStartsInProposed();
        PolicyEntryMovesToApproved();
        PolicyEntryRejectsMovesFromWithdrawn();
    }
}
