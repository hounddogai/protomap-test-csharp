namespace Acme.Workforce.Absence.Tests.Policy.Score;

using Acme.Workforce.Absence.Policy.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class PolicyManifestChecks
{
    public static void PolicyManifestStartsInProposed()
    {
        var item = PolicyManifest.NewPolicyManifest("code-475", "name-254");
        Check.Equal(item.Status, PolicyManifestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyManifestMovesToApproved()
    {
        var item = PolicyManifest.NewPolicyManifest("code-475", "name-254");
        try
        {
            item.MoveTo(PolicyManifestStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyManifestStatus.Approved, "status");
    }

    public static void PolicyManifestRejectsMovesFromWithdrawn()
    {
        var item = PolicyManifest.NewPolicyManifest("code-475", "name-254");
        item.Status = PolicyManifestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PolicyManifestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PolicyManifestStartsInProposed();
        PolicyManifestMovesToApproved();
        PolicyManifestRejectsMovesFromWithdrawn();
    }
}
