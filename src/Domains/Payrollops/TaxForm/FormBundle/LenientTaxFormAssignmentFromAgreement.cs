namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

using Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

/// <summary>Functions of lenient tax form assignment from agreement.</summary>
public static class LenientTaxFormAssignmentFromAgreement
{
    public static Dictionary<string, double> SummarizeRegionalGarnishmentAgreementsForAssignment(List<RegionalGarnishmentAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<LenientTaxFormAssignment> DeriveLenientTaxFormAssignmentsFromAgreement(List<RegionalGarnishmentAgreement> items)
    {
        var outValue = new List<LenientTaxFormAssignment>();
        foreach (var item in items)
        {
            var derived = LenientTaxFormAssignment.NewLenientTaxFormAssignment(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateLenientTaxFormAssignment(LenientTaxFormAssignment item)
    {
        return RegionalGarnishmentAgreementPolicy.RegionalGarnishmentAgreementBucket(item.ProjectedVolume());
    }
}
