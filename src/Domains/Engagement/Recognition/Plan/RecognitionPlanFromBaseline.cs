namespace Acme.Workforce.Engagement.Recognition.Plan;

using Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>Functions of recognition plan from baseline.</summary>
public static class RecognitionPlanFromBaseline
{
    public static Dictionary<string, double> SummarizeActionPlanBaselinesForPlan(List<ActionPlanBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<RecognitionPlan> DeriveRecognitionPlansFromBaseline(List<ActionPlanBaseline> items)
    {
        var outValue = new List<RecognitionPlan>();
        foreach (var item in items)
        {
            var derived = RecognitionPlan.NewRecognitionPlan(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeRecognitionPlan(RecognitionPlan item)
    {
        return ManualActionPlanMilestonePolicy.ManualActionPlanMilestoneBucket(item.MeasuredValue());
    }
}
