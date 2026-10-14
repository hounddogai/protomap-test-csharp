namespace Acme.Workforce.Absence.Tests.Policy.RollingPolicyOverride;

using Acme.Workforce.Absence.Policy.RollingPolicyOverride;

/// <summary>Checks of rolling policy override code, run by the project's runner.</summary>
public static class PolicyDigestChecks
{
    public static void PolicyDigestStartsInOpen()
    {
        var item = PolicyDigest.NewPolicyDigest("code-303", "name-929");
        Check.Equal(item.Status, PolicyDigestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyDigestMovesToResolved()
    {
        var item = PolicyDigest.NewPolicyDigest("code-303", "name-929");
        try
        {
            item.MoveTo(PolicyDigestStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyDigestStatus.Resolved, "status");
    }

    public static void PolicyDigestRejectsMovesFromArchived()
    {
        var item = PolicyDigest.NewPolicyDigest("code-303", "name-929");
        item.Status = PolicyDigestStatus.Archived;
        Check.Fails(() => item.MoveTo(PolicyDigestStatus.Open), "move");
    }

    public static void RunAll()
    {
        PolicyDigestStartsInOpen();
        PolicyDigestMovesToResolved();
        PolicyDigestRejectsMovesFromArchived();
    }
}
