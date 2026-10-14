namespace Acme.Workforce.Payrollops.PayRun.Target;

using Acme.Workforce.Payrollops.RetroPay.Draft;

/// <summary>Functions of legacy pay run target from milestone.</summary>
public static class LegacyPayRunTargetFromMilestone
{
    public static Dictionary<string, double> SummarizeRetroPayMilestonesForTarget(List<RetroPayMilestone> items)
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

    public static List<LegacyPayRunTarget> DeriveLegacyPayRunTargetsFromMilestone(List<RetroPayMilestone> items)
    {
        var outValue = new List<LegacyPayRunTarget>();
        foreach (var item in items)
        {
            var derived = LegacyPayRunTarget.NewLegacyPayRunTarget(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyLegacyPayRunTargetExtra(LegacyPayRunTarget item)
    {
        return RetroPayDraftPolicy.RetroPayDraftBucket(item.MeasuredValue());
    }
}
