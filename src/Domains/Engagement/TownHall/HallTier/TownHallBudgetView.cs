namespace Acme.Workforce.Engagement.TownHall.HallTier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall budget.</summary>
public sealed record TownHallBudgetView(string CodeText, string Name, string FloorValue, string RevisionText, string CostCenterText, string WeightText, string StartsAtText, string DescriptionText, string StatusLabel)
{
    public static TownHallBudgetView ToTownHallBudgetView(TownHallBudget item)
    {
        return new TownHallBudgetView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.CostCenter, 40),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallBudgetView> ToTownHallBudgetViews(List<TownHallBudget> items)
    {
        var views = new List<TownHallBudgetView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallBudgetView(item));
        }
        return views;
    }

    public static double SumTownHallBudgetView(TownHallBudgetView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedRevision + parsedWeight;
    }
}
