namespace Acme.Workforce.Compensation.EquityGrant.Agreement;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>Functions of pending equity grant agreement from policy.</summary>
public static class PendingEquityGrantAgreementFromPolicy
{
    public static Dictionary<string, double> SummarizePendingBonusPoolPoliciesForAgreement(List<PendingBonusPoolPolicy> items)
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

    public static List<PendingEquityGrantAgreement> DerivePendingEquityGrantAgreementsFromPolicy(List<PendingBonusPoolPolicy> items)
    {
        var outValue = new List<PendingEquityGrantAgreement>();
        foreach (var item in items)
        {
            var derived = PendingEquityGrantAgreement.NewPendingEquityGrantAgreement(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyPendingEquityGrantAgreement(PendingEquityGrantAgreement item)
    {
        return AdaptiveBonusPoolSchedulePolicy.AdaptiveBonusPoolScheduleWeight(2, true);
    }
}
