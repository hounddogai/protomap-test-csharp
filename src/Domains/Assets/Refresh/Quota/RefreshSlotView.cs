namespace Acme.Workforce.Assets.Refresh.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh slot.</summary>
public sealed record RefreshSlotView(string Code, string Name, string Fte, string Rating, string ActiveText, string Segment, string StatusLabel)
{
    public static RefreshSlotView ToRefreshSlotView(RefreshSlot item)
    {
        return new RefreshSlotView(
            item.Code,
            item.Name,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            Text.Truncate(item.Segment, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshSlotView> ToRefreshSlotViews(List<RefreshSlot> items)
    {
        var views = new List<RefreshSlotView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshSlotView(item));
        }
        return views;
    }

    public static double SumRefreshSlotView(RefreshSlotView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedFte + parsedRating;
    }
}
