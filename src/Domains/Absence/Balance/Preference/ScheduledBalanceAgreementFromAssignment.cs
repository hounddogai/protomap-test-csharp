namespace Acme.Workforce.Absence.Balance.Preference;

using Acme.Workforce.Absence.Policy.Assignment;

/// <summary>Functions of scheduled balance agreement from assignment.</summary>
public static class ScheduledBalanceAgreementFromAssignment
{
    public static Dictionary<string, double> SummarizeDefaultPolicyAssignmentsForAgreement(List<DefaultPolicyAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<ScheduledBalanceAgreement> DeriveScheduledBalanceAgreementsFromAssignment(List<DefaultPolicyAssignment> items)
    {
        var outValue = new List<ScheduledBalanceAgreement>();
        foreach (var item in items)
        {
            var derived = ScheduledBalanceAgreement.NewScheduledBalanceAgreement(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchScheduledBalanceAgreement(ScheduledBalanceAgreement item)
    {
        return FallbackPolicyScorePolicy.FallbackPolicyScoreWeight(9, true);
    }
}
