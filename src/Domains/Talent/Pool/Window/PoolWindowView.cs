namespace Acme.Workforce.Talent.Pool.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool window.</summary>
public sealed record PoolWindowView(string Code, string TitleText, string Hours, string ManagerId, string StartsAt, string Weight, string BadgeText, string Visible, string StatusLabel)
{
    public static PoolWindowView ToPoolWindowView(PoolWindow item)
    {
        return new PoolWindowView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolWindowView> ToPoolWindowViews(List<PoolWindow> items)
    {
        var views = new List<PoolWindowView>();
        foreach (var item in items)
        {
            views.Add(ToPoolWindowView(item));
        }
        return views;
    }

    public static double SumPoolWindowView(PoolWindowView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedStartsAt + parsedWeight;
    }
}
