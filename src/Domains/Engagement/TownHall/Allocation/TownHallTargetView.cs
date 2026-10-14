namespace Acme.Workforce.Engagement.TownHall.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall target.</summary>
public sealed record TownHallTargetView(string CodeText, string Title, string Rate, string Timezone, string UpdatedAt, string Retries, string Quantity, string GraceDays, string JobCode, string Channel, string StatusLabel)
{
    public static TownHallTargetView ToTownHallTargetView(TownHallTarget item)
    {
        return new TownHallTargetView(
            item.Code,
            item.Title,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 120),
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallTargetView> ToTownHallTargetViews(List<TownHallTarget> items)
    {
        var views = new List<TownHallTargetView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallTargetView(item));
        }
        return views;
    }

    public static double SumTownHallTargetView(TownHallTargetView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedUpdatedAt + (double)parsedRetries;
    }
}
