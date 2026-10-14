namespace Acme.Workforce.Engagement.Recognition.Score;

using Acme.Workforce.Engagement.Feedback.Proposal;

/// <summary>Functions of recognition policy from proposal.</summary>
public static class RecognitionPolicyFromProposal
{
    public static Dictionary<string, double> SummarizeFeedbackProposalsForPolicy(List<FeedbackProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<RecognitionPolicy> DeriveRecognitionPoliciesFromProposal(List<FeedbackProposal> items)
    {
        var outValue = new List<RecognitionPolicy>();
        foreach (var item in items)
        {
            var derived = RecognitionPolicy.NewRecognitionPolicy(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveRecognitionPolicy(RecognitionPolicy item)
    {
        return FeedbackMilestonePolicy.FeedbackMilestoneWeight(4, true);
    }
}
