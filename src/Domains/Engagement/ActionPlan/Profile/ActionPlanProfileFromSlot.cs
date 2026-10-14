namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Engagement.Recognition.RecordItem;

/// <summary>Functions of action plan profile from slot.</summary>
public static class ActionPlanProfileFromSlot
{
    public static Dictionary<string, double> SummarizeBatchedRecognitionSlotsForProfile(List<BatchedRecognitionSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<ActionPlanProfile> DeriveActionPlanProfilesFromSlot(List<BatchedRecognitionSlot> items)
    {
        var outValue = new List<ActionPlanProfile>();
        foreach (var item in items)
        {
            var derived = ActionPlanProfile.NewActionPlanProfile(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveActionPlanProfile(ActionPlanProfile item)
    {
        return CappedRecognitionThresholdPolicy.CappedRecognitionThresholdWeight(8, true);
    }
}
