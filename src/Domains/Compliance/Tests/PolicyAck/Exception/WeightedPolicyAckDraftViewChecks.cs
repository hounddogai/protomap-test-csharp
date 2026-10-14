namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class WeightedPolicyAckDraftViewChecks
{
    public static void WeightedPolicyAckDraftViewShowsTheStatusInCapitals()
    {
        var item = WeightedPolicyAckDraft.NewWeightedPolicyAckDraft("code-73", "title-949");
        var view = WeightedPolicyAckDraftView.ToWeightedPolicyAckDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedPolicyAckDraftView.ToWeightedPolicyAckDraftViews(new List<WeightedPolicyAckDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedPolicyAckDraftViewShowsTheStatusInCapitals();
    }
}
