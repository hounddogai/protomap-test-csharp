namespace Acme.Workforce.Engagement.TownHall.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall milestone.</summary>
public sealed record TownHallMilestoneView(string Code, string Label, string Weight, string Category, string Minimum, string BatchSize, string Maximum, string Notes, string Fte, string StatusLabel)
{
    public static TownHallMilestoneView ToTownHallMilestoneView(TownHallMilestone item)
    {
        return new TownHallMilestoneView(
            item.Code,
            item.Label,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 80),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 80),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallMilestoneView> ToTownHallMilestoneViews(List<TownHallMilestone> items)
    {
        var views = new List<TownHallMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallMilestoneView(item));
        }
        return views;
    }

    public static double SumTownHallMilestoneView(TownHallMilestoneView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedWeight + parsedMinimum + (double)parsedBatchSize;
    }
}
