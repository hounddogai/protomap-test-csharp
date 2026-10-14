namespace Acme.Workforce.Contractors.Tests.Engagement.Policy;

using Acme.Workforce.Contractors.Engagement.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class LenientEngagementPolicyChecks
{
    public static void LenientEngagementPolicyStartsInPlanned()
    {
        var item = LenientEngagementPolicy.NewLenientEngagementPolicy("code-332", "title-759");
        Check.Equal(item.Status, LenientEngagementPolicyStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientEngagementPolicyMovesToCompleted()
    {
        var item = LenientEngagementPolicy.NewLenientEngagementPolicy("code-332", "title-759");
        try
        {
            item.MoveTo(LenientEngagementPolicyStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientEngagementPolicyStatus.Completed, "status");
    }

    public static void LenientEngagementPolicyRejectsMovesFromCancelled()
    {
        var item = LenientEngagementPolicy.NewLenientEngagementPolicy("code-332", "title-759");
        item.Status = LenientEngagementPolicyStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientEngagementPolicyStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientEngagementPolicyStartsInPlanned();
        LenientEngagementPolicyMovesToCompleted();
        LenientEngagementPolicyRejectsMovesFromCancelled();
    }
}
