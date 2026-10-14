namespace Acme.Workforce.Absence.Tests.Policy.PolicyRule;

using Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>Checks of policy rule code, run by the project's runner.</summary>
public static class DefaultPolicyItemChecks
{
    public static void DefaultPolicyItemStartsInCreated()
    {
        var item = DefaultPolicyItem.NewDefaultPolicyItem("code-7", "name-823");
        Check.Equal(item.Status, DefaultPolicyItemStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPolicyItemMovesToRetired()
    {
        var item = DefaultPolicyItem.NewDefaultPolicyItem("code-7", "name-823");
        try
        {
            item.MoveTo(DefaultPolicyItemStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPolicyItemStatus.Retired, "status");
    }

    public static void DefaultPolicyItemRejectsMovesFromRetired()
    {
        var item = DefaultPolicyItem.NewDefaultPolicyItem("code-7", "name-823");
        item.Status = DefaultPolicyItemStatus.Retired;
        Check.Fails(() => item.MoveTo(DefaultPolicyItemStatus.Created), "move");
    }

    public static void RunAll()
    {
        DefaultPolicyItemStartsInCreated();
        DefaultPolicyItemMovesToRetired();
        DefaultPolicyItemRejectsMovesFromRetired();
    }
}
