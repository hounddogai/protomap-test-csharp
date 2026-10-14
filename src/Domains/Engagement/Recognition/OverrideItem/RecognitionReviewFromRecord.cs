namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

using Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>Functions of recognition review from record.</summary>
public static class RecognitionReviewFromRecord
{
    public static Dictionary<string, double> SummarizeActionPlanRecordsForReview(List<ActionPlanRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<RecognitionReview> DeriveRecognitionReviewsFromRecord(List<ActionPlanRecord> items)
    {
        var outValue = new List<RecognitionReview>();
        foreach (var item in items)
        {
            var derived = RecognitionReview.NewRecognitionReview(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateRecognitionReview(RecognitionReview item)
    {
        return ActionPlanDigestPolicy.ActionPlanDigestWeight(5, true);
    }
}
