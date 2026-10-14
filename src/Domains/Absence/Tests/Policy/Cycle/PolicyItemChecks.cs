namespace Acme.Workforce.Absence.Tests.Policy.Cycle;

using Acme.Workforce.Absence.Policy.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PolicyItemChecks
{
    public static void PolicyItemStartsInCreated()
    {
        var item = PolicyItem.NewPolicyItem("code-60", "label-718");
        Check.Equal(item.Status, PolicyItemStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyItemMovesToValidated()
    {
        var item = PolicyItem.NewPolicyItem("code-60", "label-718");
        try
        {
            item.MoveTo(PolicyItemStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyItemStatus.Validated, "status");
    }

    public static void PolicyItemRejectsMovesFromRetired()
    {
        var item = PolicyItem.NewPolicyItem("code-60", "label-718");
        item.Status = PolicyItemStatus.Retired;
        Check.Fails(() => item.MoveTo(PolicyItemStatus.Created), "move");
    }

    public static void RunAll()
    {
        PolicyItemStartsInCreated();
        PolicyItemMovesToValidated();
        PolicyItemRejectsMovesFromRetired();
    }
}
