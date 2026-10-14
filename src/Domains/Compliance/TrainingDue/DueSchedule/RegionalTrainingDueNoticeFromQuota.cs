namespace Acme.Workforce.Compliance.TrainingDue.DueSchedule;

using Acme.Workforce.Compliance.Exception.Audit;

/// <summary>Functions of regional training due notice from quota.</summary>
public static class RegionalTrainingDueNoticeFromQuota
{
    public static Dictionary<string, double> SummarizeDefaultExceptionQuotasForNotice(List<DefaultExceptionQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<RegionalTrainingDueNotice> DeriveRegionalTrainingDueNoticesFromQuota(List<DefaultExceptionQuota> items)
    {
        var outValue = new List<RegionalTrainingDueNotice>();
        foreach (var item in items)
        {
            var derived = RegionalTrainingDueNotice.NewRegionalTrainingDueNotice(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreRegionalTrainingDueNotice(RegionalTrainingDueNotice item)
    {
        return DefaultExceptionQuotaPolicy.DefaultExceptionQuotaBucket(item.MeasuredValue());
    }
}
