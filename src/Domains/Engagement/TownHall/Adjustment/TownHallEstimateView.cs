namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a town hall estimate.</summary>
public sealed record TownHallEstimateView(string Code, string NameText, string PinnedText, string LeaveDaysText, string Active, string GraceDays, string StatusLabel)
{
    public static TownHallEstimateView ToTownHallEstimateView(TownHallEstimate item)
    {
        return new TownHallEstimateView(
            item.Code,
            item.Name,
            $"{item.Pinned}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallEstimateView> ToTownHallEstimateViews(List<TownHallEstimate> items)
    {
        var views = new List<TownHallEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallEstimateView(item));
        }
        return views;
    }

    public static double SumTownHallEstimateView(TownHallEstimateView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedGraceDays;
    }
}
