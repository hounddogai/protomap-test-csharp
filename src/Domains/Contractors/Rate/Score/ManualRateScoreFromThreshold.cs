namespace Acme.Workforce.Contractors.Rate.Score;

using Acme.Workforce.Contractors.Offboarding.Mapping;

/// <summary>Functions of manual rate score from threshold.</summary>
public static class ManualRateScoreFromThreshold
{
    public static Dictionary<string, double> SummarizeWeightedOffboardingThresholdsForScore(List<WeightedOffboardingThreshold> items)
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

    public static List<ManualRateScore> DeriveManualRateScoresFromThreshold(List<WeightedOffboardingThreshold> items)
    {
        var outValue = new List<ManualRateScore>();
        foreach (var item in items)
        {
            var derived = ManualRateScore.NewManualRateScore(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeManualRateScore(ManualRateScore item)
    {
        return OffboardingRecordPolicy.OffboardingRecordBucket(item.WeightedVolume());
    }
}
