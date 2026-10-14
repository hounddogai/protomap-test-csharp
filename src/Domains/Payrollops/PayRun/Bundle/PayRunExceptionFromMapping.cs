namespace Acme.Workforce.Payrollops.PayRun.Bundle;

using Acme.Workforce.Payrollops.TaxForm.Entry;

/// <summary>Functions of pay run exception from mapping.</summary>
public static class PayRunExceptionFromMapping
{
    public static Dictionary<string, double> SummarizeTaxFormMappingsForException(List<TaxFormMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<PayRunException> DerivePayRunExceptionsFromMapping(List<TaxFormMapping> items)
    {
        var outValue = new List<PayRunException>();
        foreach (var item in items)
        {
            var derived = PayRunException.NewPayRunException(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolvePayRunException(PayRunException item)
    {
        return TaxFormScorePolicy.NormalizeTaxFormScoreCode(item.Code);
    }
}
