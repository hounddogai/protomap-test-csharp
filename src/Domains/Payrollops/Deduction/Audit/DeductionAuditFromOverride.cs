namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Payrollops.Deduction.DeductionBudget;

/// <summary>Functions of deduction audit from override.</summary>
public static class DeductionAuditFromOverride
{
    public static Dictionary<string, double> SummarizePrimaryDeductionOverridesForAudit(List<PrimaryDeductionOverride> items)
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

    public static List<DeductionAudit> DeriveDeductionAuditsFromOverride(List<PrimaryDeductionOverride> items)
    {
        var outValue = new List<DeductionAudit>();
        foreach (var item in items)
        {
            var derived = DeductionAudit.NewDeductionAudit(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleDeductionAudit(DeductionAudit item)
    {
        return PrimaryDeductionOverridePolicy.NormalizePrimaryDeductionOverrideCode(item.Code);
    }
}
