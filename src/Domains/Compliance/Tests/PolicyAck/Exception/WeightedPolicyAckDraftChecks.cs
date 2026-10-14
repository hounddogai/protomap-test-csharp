namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class WeightedPolicyAckDraftChecks
{
    public static void WeightedPolicyAckDraftStartsInCreated()
    {
        var item = WeightedPolicyAckDraft.NewWeightedPolicyAckDraft("code-73", "title-949");
        Check.Equal(item.Status, WeightedPolicyAckDraftStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedPolicyAckDraftMovesToPublished()
    {
        var item = WeightedPolicyAckDraft.NewWeightedPolicyAckDraft("code-73", "title-949");
        try
        {
            item.MoveTo(WeightedPolicyAckDraftStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedPolicyAckDraftStatus.Published, "status");
    }

    public static void WeightedPolicyAckDraftRejectsMovesFromRetired()
    {
        var item = WeightedPolicyAckDraft.NewWeightedPolicyAckDraft("code-73", "title-949");
        item.Status = WeightedPolicyAckDraftStatus.Retired;
        Check.Fails(() => item.MoveTo(WeightedPolicyAckDraftStatus.Created), "move");
    }

    public static void RunAll()
    {
        WeightedPolicyAckDraftStartsInCreated();
        WeightedPolicyAckDraftMovesToPublished();
        WeightedPolicyAckDraftRejectsMovesFromRetired();
    }
}
