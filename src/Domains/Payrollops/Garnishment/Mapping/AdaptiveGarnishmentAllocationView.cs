namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive garnishment allocation.</summary>
public sealed record AdaptiveGarnishmentAllocationView(string CodeText, string TitleText, string Name, string Category, string ManagerIdText, string AmountText, string StatusLabel)
{
    public static AdaptiveGarnishmentAllocationView ToAdaptiveGarnishmentAllocationView(AdaptiveGarnishmentAllocation item)
    {
        return new AdaptiveGarnishmentAllocationView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Name,
            Text.Truncate(item.Category, 40),
            item.ManagerId,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveGarnishmentAllocationView> ToAdaptiveGarnishmentAllocationViews(List<AdaptiveGarnishmentAllocation> items)
    {
        var views = new List<AdaptiveGarnishmentAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveGarnishmentAllocationView(item));
        }
        return views;
    }

    public static double SumAdaptiveGarnishmentAllocationView(AdaptiveGarnishmentAllocationView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
