namespace Acme.Workforce.Absence.Tests.Policy.PolicyOverride;

using Acme.Workforce.Absence.Policy.PolicyOverride;

/// <summary>Checks of policy override code, run by the project's runner.</summary>
public static class PolicyTargetChecks
{
    public static void PolicyTargetStartsInCreated()
    {
        var item = PolicyTarget.NewPolicyTarget("code-635", "title-538");
        Check.Equal(item.Status, PolicyTargetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyTargetMovesToRetired()
    {
        var item = PolicyTarget.NewPolicyTarget("code-635", "title-538");
        try
        {
            item.MoveTo(PolicyTargetStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyTargetStatus.Retired, "status");
    }

    public static void PolicyTargetRejectsMovesFromRetired()
    {
        var item = PolicyTarget.NewPolicyTarget("code-635", "title-538");
        item.Status = PolicyTargetStatus.Retired;
        Check.Fails(() => item.MoveTo(PolicyTargetStatus.Created), "move");
    }

    public static void RunAll()
    {
        PolicyTargetStartsInCreated();
        PolicyTargetMovesToRetired();
        PolicyTargetRejectsMovesFromRetired();
    }
}
