namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.TrainingDue.Summary;

/// <summary>Functions of seasonal exception manifest from summary.</summary>
public static class SeasonalExceptionManifestFromSummary
{
    public static Dictionary<string, double> SummarizeTrainingDueSummariesForManifest(List<TrainingDueSummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<SeasonalExceptionManifest> DeriveSeasonalExceptionManifestsFromSummary(List<TrainingDueSummary> items)
    {
        var outValue = new List<SeasonalExceptionManifest>();
        foreach (var item in items)
        {
            var derived = SeasonalExceptionManifest.NewSeasonalExceptionManifest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleSeasonalExceptionManifest(SeasonalExceptionManifest item)
    {
        return TrainingDueTemplatePolicy.TrainingDueTemplateWeight(5, true);
    }
}
