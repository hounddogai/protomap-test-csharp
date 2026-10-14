namespace Acme.Workforce.Payrollops.PayRun.RunRecord;

using Acme.Workforce.Payrollops.TaxForm.Exception;

/// <summary>Functions of pay run rule from exception.</summary>
public static class PayRunRuleFromException
{
    public static Dictionary<string, double> SummarizeWeightedTaxFormExceptionsForRule(List<WeightedTaxFormException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<PayRunRule> DerivePayRunRulesFromException(List<WeightedTaxFormException> items)
    {
        var outValue = new List<PayRunRule>();
        foreach (var item in items)
        {
            var derived = PayRunRule.NewPayRunRule(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcilePayRunRule(PayRunRule item)
    {
        return WeightedTaxFormExceptionPolicy.NormalizeWeightedTaxFormExceptionCode(item.Code);
    }
}
