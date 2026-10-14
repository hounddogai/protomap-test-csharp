namespace Acme.Workforce.Engagement.TownHall.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy town hall cycle.</summary>
public sealed record LegacyTownHallCycleView(string Code, string Title, string Quantity, string ApprovedText, string Summary, string ManagerId, string StatusLabel)
{
    public static LegacyTownHallCycleView ToLegacyTownHallCycleView(LegacyTownHallCycle item)
    {
        return new LegacyTownHallCycleView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Summary,
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyTownHallCycleView> ToLegacyTownHallCycleViews(List<LegacyTownHallCycle> items)
    {
        var views = new List<LegacyTownHallCycleView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyTownHallCycleView(item));
        }
        return views;
    }

    public static double SumLegacyTownHallCycleView(LegacyTownHallCycleView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
