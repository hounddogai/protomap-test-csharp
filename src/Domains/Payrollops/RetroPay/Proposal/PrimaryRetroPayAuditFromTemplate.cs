namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

using Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Functions of primary retro pay audit from template.</summary>
public static class PrimaryRetroPayAuditFromTemplate
{
    public static Dictionary<string, double> SummarizePrimaryDeductionTemplatesForAudit(List<PrimaryDeductionTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<PrimaryRetroPayAudit> DerivePrimaryRetroPayAuditsFromTemplate(List<PrimaryDeductionTemplate> items)
    {
        var outValue = new List<PrimaryRetroPayAudit>();
        foreach (var item in items)
        {
            var derived = PrimaryRetroPayAudit.NewPrimaryRetroPayAudit(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitPrimaryRetroPayAudit(PrimaryRetroPayAudit item)
    {
        return DeductionAuditPolicy.ClassifyDeductionAudit(item.MeasuredValue());
    }
}
