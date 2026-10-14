namespace Acme.Workforce.Contractors.Offboarding.Mapping;

using Acme.Workforce.Contractors.StatementOfWork.Group;

/// <summary>Functions of weighted offboarding threshold from slot.</summary>
public static class WeightedOffboardingThresholdFromSlot
{
    public static Dictionary<string, double> SummarizeLenientStatementOfWorkSlotsForThreshold(List<LenientStatementOfWorkSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<WeightedOffboardingThreshold> DeriveWeightedOffboardingThresholdsFromSlot(List<LenientStatementOfWorkSlot> items)
    {
        var outValue = new List<WeightedOffboardingThreshold>();
        foreach (var item in items)
        {
            var derived = WeightedOffboardingThreshold.NewWeightedOffboardingThreshold(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreWeightedOffboardingThreshold(WeightedOffboardingThreshold item)
    {
        return LenientStatementOfWorkSlotPolicy.ClassifyLenientStatementOfWorkSlot(item.AdjustedVolume());
    }
}
