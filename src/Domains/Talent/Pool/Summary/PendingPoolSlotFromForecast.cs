namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Talent.SkillProfile.Notice;

/// <summary>Functions of pending pool slot from forecast.</summary>
public static class PendingPoolSlotFromForecast
{
    public static Dictionary<string, double> SummarizePendingSkillProfileForecastsForSlot(List<PendingSkillProfileForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<PendingPoolSlot> DerivePendingPoolSlotsFromForecast(List<PendingSkillProfileForecast> items)
    {
        var outValue = new List<PendingPoolSlot>();
        foreach (var item in items)
        {
            var derived = PendingPoolSlot.NewPendingPoolSlot(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyPendingPoolSlot(PendingPoolSlot item)
    {
        return PendingSkillProfileForecastPolicy.ClassifyPendingSkillProfileForecast(item.NetVolume());
    }
}
