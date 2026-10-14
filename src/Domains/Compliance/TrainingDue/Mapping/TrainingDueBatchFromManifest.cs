namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Functions of training due batch from manifest.</summary>
public static class TrainingDueBatchFromManifest
{
    public static Dictionary<string, double> SummarizeSeasonalExceptionManifestsForBatch(List<SeasonalExceptionManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<TrainingDueBatch> DeriveTrainingDueBatchesFromManifest(List<SeasonalExceptionManifest> items)
    {
        var outValue = new List<TrainingDueBatch>();
        foreach (var item in items)
        {
            var derived = TrainingDueBatch.NewTrainingDueBatch(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceTrainingDueBatch(TrainingDueBatch item)
    {
        return ExceptionSummaryPolicy.ExceptionSummaryWeight(4, true);
    }
}
