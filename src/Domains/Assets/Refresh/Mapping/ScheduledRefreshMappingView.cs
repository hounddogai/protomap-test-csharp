namespace Acme.Workforce.Assets.Refresh.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled refresh mapping.</summary>
public sealed record ScheduledRefreshMappingView(string Code, string LabelText, string MinimumText, string Discount, string Name, string SalaryBandText, string TitleText, string Rate, string Team, string Category, string StatusLabel)
{
    public static ScheduledRefreshMappingView ToScheduledRefreshMappingView(ScheduledRefreshMapping item)
    {
        return new ScheduledRefreshMappingView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.SalaryBand,
            Text.Truncate(item.Title, 120),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            Text.Truncate(item.Category, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledRefreshMappingView> ToScheduledRefreshMappingViews(List<ScheduledRefreshMapping> items)
    {
        var views = new List<ScheduledRefreshMappingView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledRefreshMappingView(item));
        }
        return views;
    }

    public static double SumScheduledRefreshMappingView(ScheduledRefreshMappingView view)
    {
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedDiscount + parsedRate;
    }
}
