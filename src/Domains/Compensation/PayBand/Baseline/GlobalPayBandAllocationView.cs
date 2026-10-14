namespace Acme.Workforce.Compensation.PayBand.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global pay band allocation.</summary>
public sealed record GlobalPayBandAllocationView(string Code, string Title, string Channel, string Phone, string Visible, string Cost, string Category, string Currency, string StatusLabel)
{
    public static GlobalPayBandAllocationView ToGlobalPayBandAllocationView(GlobalPayBandAllocation item)
    {
        return new GlobalPayBandAllocationView(
            item.Code,
            item.Title,
            item.Channel,
            item.Phone,
            $"{item.Visible}",
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 40),
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalPayBandAllocationView> ToGlobalPayBandAllocationViews(List<GlobalPayBandAllocation> items)
    {
        var views = new List<GlobalPayBandAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalPayBandAllocationView(item));
        }
        return views;
    }

    public static double SumGlobalPayBandAllocationView(GlobalPayBandAllocationView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedCost;
    }
}
