namespace Acme.Workforce.Engagement.Recognition.Request;

using Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>Functions of recognition request from request.</summary>
public static class RecognitionRequestFromRequest
{
    public static Dictionary<string, double> SummarizeActionPlanRequestsForRequest(List<ActionPlanRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<RecognitionRequest> DeriveRecognitionRequestsFromRequest(List<ActionPlanRequest> items)
    {
        var outValue = new List<RecognitionRequest>();
        foreach (var item in items)
        {
            var derived = RecognitionRequest.NewRecognitionRequest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankRecognitionRequest(RecognitionRequest item)
    {
        return ActionPlanEntryPolicy.ActionPlanEntryBucket(item.WeightedValue());
    }
}
