namespace Acme.Workforce.Payrollops.Payslip.PayslipDigest;

using Acme.Workforce.Payrollops.TaxForm.FormTemplate;

/// <summary>Functions of payslip score from manifest.</summary>
public static class PayslipScoreFromManifest
{
    public static Dictionary<string, double> SummarizeTaxFormManifestsForScore(List<TaxFormManifest> items)
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

    public static List<PayslipScore> DerivePayslipScoresFromManifest(List<TaxFormManifest> items)
    {
        var outValue = new List<PayslipScore>();
        foreach (var item in items)
        {
            var derived = PayslipScore.NewPayslipScore(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchPayslipScore(PayslipScore item)
    {
        return TaxFormTierPolicy.NormalizeTaxFormTierCode(item.Code);
    }
}
