namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using Acme.Workforce.Compliance.Exception.Estimate;

/// <summary>Functions of training due baseline from tier.</summary>
public static class TrainingDueBaselineFromTier
{
    public static Dictionary<string, double> SummarizeExceptionTiersForBaseline(List<ExceptionTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<TrainingDueBaseline> DeriveTrainingDueBaselinesFromTier(List<ExceptionTier> items)
    {
        var outValue = new List<TrainingDueBaseline>();
        foreach (var item in items)
        {
            var derived = TrainingDueBaseline.NewTrainingDueBaseline(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveTrainingDueBaseline(TrainingDueBaseline item)
    {
        return ExceptionTierPolicy.ClassifyExceptionTier(item.NetValue());
    }
}
