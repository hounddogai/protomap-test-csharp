namespace Acme.Workforce.Compliance.Exception.Audit;

using Acme.Workforce.Compliance.Attestation.Quota;

/// <summary>Functions of default exception quota from budget.</summary>
public static class DefaultExceptionQuotaFromBudget
{
    public static Dictionary<string, double> SummarizeAttestationBudgetsForQuota(List<AttestationBudget> items)
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

    public static List<DefaultExceptionQuota> DeriveDefaultExceptionQuotasFromBudget(List<AttestationBudget> items)
    {
        var outValue = new List<DefaultExceptionQuota>();
        foreach (var item in items)
        {
            var derived = DefaultExceptionQuota.NewDefaultExceptionQuota(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchDefaultExceptionQuota(DefaultExceptionQuota item)
    {
        return AttestationBudgetPolicy.NormalizeAttestationBudgetCode(item.Code);
    }
}
