namespace Acme.Workforce.Payrollops.PayRun.Window;

using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Functions of lenient pay run agreement from allocation.</summary>
public static class LenientPayRunAgreementFromAllocation
{
    public static Dictionary<string, double> SummarizeSeasonalTaxFormAllocationsForAgreement(List<SeasonalTaxFormAllocation> items)
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

    public static List<LenientPayRunAgreement> DeriveLenientPayRunAgreementsFromAllocation(List<SeasonalTaxFormAllocation> items)
    {
        var outValue = new List<LenientPayRunAgreement>();
        foreach (var item in items)
        {
            var derived = LenientPayRunAgreement.NewLenientPayRunAgreement(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeLenientPayRunAgreement(LenientPayRunAgreement item)
    {
        return PendingTaxFormOverridePolicy.NormalizePendingTaxFormOverrideCode(item.Code);
    }
}
