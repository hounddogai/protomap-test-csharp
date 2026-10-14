namespace Acme.Workforce.Payrollops.Garnishment.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a garnishment estimate.</summary>
public sealed record GarnishmentEstimateView(string CodeText, string NameText, string Notes, string Checksum, string Label, string Quantity, string StatusLabel)
{
    public static GarnishmentEstimateView ToGarnishmentEstimateView(GarnishmentEstimate item)
    {
        return new GarnishmentEstimateView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Notes,
            Text.Truncate(item.Checksum, 80),
            Text.Truncate(item.Label, 120),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentEstimateView> ToGarnishmentEstimateViews(List<GarnishmentEstimate> items)
    {
        var views = new List<GarnishmentEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentEstimateView(item));
        }
        return views;
    }

    public static double SumGarnishmentEstimateView(GarnishmentEstimateView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
