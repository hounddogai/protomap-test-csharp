namespace Acme.Workforce.Payrollops.PayRun.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional pay run variance.</summary>
public sealed record RegionalPayRunVarianceView(string Code, string Label, string BatchSizeText, string Active, string CountryText, string StatusLabel)
{
    public static RegionalPayRunVarianceView ToRegionalPayRunVarianceView(RegionalPayRunVariance item)
    {
        return new RegionalPayRunVarianceView(
            item.Code,
            item.Label,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            Text.Truncate(item.Country, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalPayRunVarianceView> ToRegionalPayRunVarianceViews(List<RegionalPayRunVariance> items)
    {
        var views = new List<RegionalPayRunVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalPayRunVarianceView(item));
        }
        return views;
    }

    public static double SumRegionalPayRunVarianceView(RegionalPayRunVarianceView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize;
    }
}
