namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Assets.Refresh.Review;

/// <summary>Functions of lenient laptop agreement from variance.</summary>
public static class LenientLaptopAgreementFromVariance
{
    public static Dictionary<string, double> SummarizePrimaryRefreshVariancesForAgreement(List<PrimaryRefreshVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<LenientLaptopAgreement> DeriveLenientLaptopAgreementsFromVariance(List<PrimaryRefreshVariance> items)
    {
        var outValue = new List<LenientLaptopAgreement>();
        foreach (var item in items)
        {
            var derived = LenientLaptopAgreement.NewLenientLaptopAgreement(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitLenientLaptopAgreement(LenientLaptopAgreement item)
    {
        return RefreshRecordPolicy.ClassifyRefreshRecord(item.ProjectedLoad());
    }
}
