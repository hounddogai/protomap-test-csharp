namespace Acme.Workforce.Payrollops.Garnishment.RecordItem;

using Acme.Workforce.Payrollops.TaxForm.FormTemplate;

/// <summary>Functions of capped garnishment record from template.</summary>
public static class CappedGarnishmentRecordFromTemplate
{
    public static Dictionary<string, double> SummarizePrimaryTaxFormTemplatesForRecord(List<PrimaryTaxFormTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<CappedGarnishmentRecord> DeriveCappedGarnishmentRecordsFromTemplate(List<PrimaryTaxFormTemplate> items)
    {
        var outValue = new List<CappedGarnishmentRecord>();
        foreach (var item in items)
        {
            var derived = CappedGarnishmentRecord.NewCappedGarnishmentRecord(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeCappedGarnishmentRecord(CappedGarnishmentRecord item)
    {
        return TaxFormTierPolicy.NormalizeTaxFormTierCode(item.Code);
    }
}
