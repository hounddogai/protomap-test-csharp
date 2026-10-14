namespace Acme.Workforce.Engagement.TownHall.Cycle;

using Acme.Workforce.Engagement.PulseSurvey.Exception;

/// <summary>Functions of batched town hall quota from exception.</summary>
public static class BatchedTownHallQuotaFromException
{
    public static Dictionary<string, double> SummarizePulseSurveyExceptionsForQuota(List<PulseSurveyException> items)
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

    public static List<BatchedTownHallQuota> DeriveBatchedTownHallQuotasFromException(List<PulseSurveyException> items)
    {
        var outValue = new List<BatchedTownHallQuota>();
        foreach (var item in items)
        {
            var derived = BatchedTownHallQuota.NewBatchedTownHallQuota(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateBatchedTownHallQuota(BatchedTownHallQuota item)
    {
        return PulseSurveyExceptionPolicy.NormalizePulseSurveyExceptionCode(item.Code);
    }
}
