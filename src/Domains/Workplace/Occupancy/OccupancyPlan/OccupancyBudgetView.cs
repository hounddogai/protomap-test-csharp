namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a occupancy budget.</summary>
public sealed record OccupancyBudgetView(string Code, string Label, string OwnerId, string Notes, string Phone, string Fte, string StatusLabel)
{
    public static OccupancyBudgetView ToOccupancyBudgetView(OccupancyBudget item)
    {
        return new OccupancyBudgetView(
            item.Code,
            item.Label,
            item.OwnerId,
            Text.Truncate(item.Notes, 80),
            item.Phone,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyBudgetView> ToOccupancyBudgetViews(List<OccupancyBudget> items)
    {
        var views = new List<OccupancyBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyBudgetView(item));
        }
        return views;
    }

    public static double SumOccupancyBudgetView(OccupancyBudgetView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return parsedFte;
    }
}
