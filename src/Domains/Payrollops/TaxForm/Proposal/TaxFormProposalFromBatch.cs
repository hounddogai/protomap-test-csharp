namespace Acme.Workforce.Payrollops.TaxForm.Proposal;

using Acme.Workforce.Payrollops.Garnishment.Variance;

/// <summary>Functions of tax form proposal from batch.</summary>
public static class TaxFormProposalFromBatch
{
    public static Dictionary<string, double> SummarizeGarnishmentBatchesForProposal(List<GarnishmentBatch> items)
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

    public static List<TaxFormProposal> DeriveTaxFormProposalsFromBatch(List<GarnishmentBatch> items)
    {
        var outValue = new List<TaxFormProposal>();
        foreach (var item in items)
        {
            var derived = TaxFormProposal.NewTaxFormProposal(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyTaxFormProposal(TaxFormProposal item)
    {
        return GarnishmentBatchPolicy.NormalizeGarnishmentBatchCode(item.Code);
    }
}
