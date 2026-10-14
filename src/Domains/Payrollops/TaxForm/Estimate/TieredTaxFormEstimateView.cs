namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered tax form estimate.</summary>
public sealed record TieredTaxFormEstimateView(string Code, string NameText, string Notes, string ExternalRef, string Ratio, string StatusLabel)
{
    public static TieredTaxFormEstimateView ToTieredTaxFormEstimateView(TieredTaxFormEstimate item)
    {
        return new TieredTaxFormEstimateView(
            Text.Truncate(item.Code, 120),
            item.Name,
            Text.Truncate(item.Notes, 40),
            item.ExternalRef,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredTaxFormEstimateView> ToTieredTaxFormEstimateViews(List<TieredTaxFormEstimate> items)
    {
        var views = new List<TieredTaxFormEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToTieredTaxFormEstimateView(item));
        }
        return views;
    }

    public static double SumTieredTaxFormEstimateView(TieredTaxFormEstimateView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedRatio;
    }
}
