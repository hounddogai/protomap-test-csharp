namespace Acme.Workforce.Absence.Accrual.Notice;

using Acme.Workforce.Absence.ReturnPlan.Summary;

/// <summary>Functions of primary accrual notice from override.</summary>
public static class PrimaryAccrualNoticeFromOverride
{
    public static Dictionary<string, double> SummarizeLenientReturnPlanOverridesForNotice(List<LenientReturnPlanOverride> items)
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

    public static List<PrimaryAccrualNotice> DerivePrimaryAccrualNoticesFromOverride(List<LenientReturnPlanOverride> items)
    {
        var outValue = new List<PrimaryAccrualNotice>();
        foreach (var item in items)
        {
            var derived = PrimaryAccrualNotice.NewPrimaryAccrualNotice(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyPrimaryAccrualNotice(PrimaryAccrualNotice item)
    {
        return LenientReturnPlanOverridePolicy.ClassifyLenientReturnPlanOverride(item.MeasuredValue());
    }
}
