namespace Acme.Workforce.Payrollops.RetroPay.PayBudget;

using Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>Functions of retro pay template from policy.</summary>
public static class RetroPayTemplateFromPolicy
{
    public static Dictionary<string, double> SummarizeDeductionPoliciesForTemplate(List<DeductionPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<RetroPayTemplate> DeriveRetroPayTemplatesFromPolicy(List<DeductionPolicy> items)
    {
        var outValue = new List<RetroPayTemplate>();
        foreach (var item in items)
        {
            var derived = RetroPayTemplate.NewRetroPayTemplate(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreRetroPayTemplate(RetroPayTemplate item)
    {
        return DeductionPolicyPolicy.ClassifyDeductionPolicy(item.AdjustedValue());
    }
}
