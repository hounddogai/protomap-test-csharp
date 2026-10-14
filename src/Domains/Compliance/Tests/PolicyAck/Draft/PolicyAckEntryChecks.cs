namespace Acme.Workforce.Compliance.Tests.PolicyAck.Draft;

using Acme.Workforce.Compliance.PolicyAck.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PolicyAckEntryChecks
{
    public static void PolicyAckEntryStartsInCreated()
    {
        var item = PolicyAckEntry.NewPolicyAckEntry("code-245", "label-807");
        Check.Equal(item.Status, PolicyAckEntryStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyAckEntryMovesToValidated()
    {
        var item = PolicyAckEntry.NewPolicyAckEntry("code-245", "label-807");
        try
        {
            item.MoveTo(PolicyAckEntryStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyAckEntryStatus.Validated, "status");
    }

    public static void PolicyAckEntryRejectsMovesFromRetired()
    {
        var item = PolicyAckEntry.NewPolicyAckEntry("code-245", "label-807");
        item.Status = PolicyAckEntryStatus.Retired;
        Check.Fails(() => item.MoveTo(PolicyAckEntryStatus.Created), "move");
    }

    public static void RunAll()
    {
        PolicyAckEntryStartsInCreated();
        PolicyAckEntryMovesToValidated();
        PolicyAckEntryRejectsMovesFromRetired();
    }
}
